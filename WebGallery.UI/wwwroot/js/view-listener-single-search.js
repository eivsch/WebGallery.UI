import { renderPager } from './pager.js';

const headline = document.querySelector('#single-page-headline');
const galleryGrid = document.querySelector('#single-gallery-grid');
const filterInput = document.querySelector('#single-filter-input');
const filterStatus = document.querySelector('#single-filter-status');
const filterWarning = document.querySelector('#single-filter-warning');
const deepSearchButton = document.querySelector('#single-deep-search-btn');

if (headline && galleryGrid && filterInput && filterStatus && deepSearchButton) {
    const configuredPageSize = Math.max(1, window.webGalleryDisplay?.pageSize ?? 48);
    const minimumQueryLength = Math.max(1, window.webGalleryDisplay?.singleSearchMinQueryLength ?? 5);
    const initialHeadlineText = headline.textContent ?? '';
    const initialGridHtml = galleryGrid.innerHTML;
    const photosContainer = document.querySelector('.container-fluid.photos');
    const initialPagerRow = document.querySelector('.pager-row');
    const initialPagerDisplay = initialPagerRow?.style.display ?? '';
    const initialCards = Array.from(galleryGrid.querySelectorAll('[data-single-item="true"]')).map((card) => {
        const itemAnchor = card.querySelector('a.photo-item');
        return {
            card,
            name: (itemAnchor?.getAttribute('data-name') ?? '').toLowerCase(),
            album: (itemAnchor?.getAttribute('data-album') ?? '').toLowerCase(),
            tags: (itemAnchor?.getAttribute('data-tags') ?? '').toLowerCase(),
        };
    });

    let dynamicPagerRow = null;
    let deepSearchRequestId = 0;
    let isDeepSearchMode = false;

    filterInput.addEventListener('input', debounce(() => {
        const query = (filterInput.value ?? '').trim();

        if (isDeepSearchMode) {
            restoreInitialGrid();
        }

        if (!query) {
            clearFilterUiState();
            return;
        }

        const localMatchCount = applyLocalFilter(query);
        if (query.length < minimumQueryLength) {
            deepSearchButton.disabled = true;
            filterStatus.textContent = `${localMatchCount} local match(es). Type at least ${minimumQueryLength} characters to search all items.`;
            return;
        }

        deepSearchButton.disabled = false;
        filterStatus.textContent = `${localMatchCount} local match(es). Click Search more to scan beyond preloaded items.`;
    }, 220));

    deepSearchButton.addEventListener('click', () => {
        triggerDeepSearch(1);
    });

    filterInput.addEventListener('keydown', (event) => {
        if (event.key !== 'Enter') {
            return;
        }

        event.preventDefault();
        if (!deepSearchButton.disabled) {
            triggerDeepSearch(1);
        }
    });

    function clearFilterUiState() {
        deepSearchButton.disabled = true;
        filterStatus.textContent = '';
        setWarning('');
        restoreInitialGrid();
        headline.textContent = initialHeadlineText;
    }

    function restoreInitialGrid() {
        isDeepSearchMode = false;
        galleryGrid.innerHTML = initialGridHtml;
        removeDynamicPager();
        setInitialPagerVisible(true);
    }

    function setWarning(message) {
        if (!filterWarning) {
            return;
        }

        if (!message) {
            filterWarning.style.display = 'none';
            filterWarning.textContent = '';
            return;
        }

        filterWarning.style.display = '';
        filterWarning.textContent = message;
    }

    function setInitialPagerVisible(visible) {
        if (!initialPagerRow) {
            return;
        }

        initialPagerRow.style.display = visible ? initialPagerDisplay : 'none';
    }

    function removeDynamicPager() {
        if (!dynamicPagerRow) {
            return;
        }

        dynamicPagerRow.remove();
        dynamicPagerRow = null;
    }

    function applyLocalFilter(query) {
        const normalizedQuery = query.toLowerCase();
        const matchedCards = initialCards.filter((item) => {
            const haystack = `${item.name} ${item.album} ${item.tags}`;
            return haystack.includes(normalizedQuery);
        });

        galleryGrid.innerHTML = '';
        if (matchedCards.length === 0) {
            galleryGrid.innerHTML = createEmptyStateMarkup('No local matches found.');
        }
        else {
            matchedCards.forEach((item) => {
                galleryGrid.appendChild(item.card.cloneNode(true));
            });
        }

        headline.textContent = `'Single' - ${matchedCards.length} local match(es)`;
        setInitialPagerVisible(false);
        return matchedCards.length;
    }

    async function triggerDeepSearch(page) {
        const query = (filterInput.value ?? '').trim();
        if (query.length < minimumQueryLength) {
            deepSearchButton.disabled = true;
            filterStatus.textContent = `Type at least ${minimumQueryLength} characters to search all items.`;
            return;
        }

        const requestId = ++deepSearchRequestId;
        deepSearchButton.disabled = true;
        filterStatus.textContent = 'Searching all items...';
        setWarning('');

        try {
            const response = await fetch(`/data/single/search?q=${encodeURIComponent(query)}&page=${page}`);
            if (!response.ok) {
                throw new Error('Search request failed');
            }

            const payload = await response.json();
            if (requestId !== deepSearchRequestId) {
                return;
            }

            if (payload.queryTooShort) {
                filterStatus.textContent = `Type at least ${payload.minimumQueryLength} characters to search all items.`;
                return;
            }

            isDeepSearchMode = true;
            renderDeepSearchResults(payload, query);
        }
        catch {
            if (requestId !== deepSearchRequestId) {
                return;
            }

            filterStatus.textContent = 'Unable to perform deep search right now.';
        }
        finally {
            if (requestId === deepSearchRequestId) {
                deepSearchButton.disabled = query.length < minimumQueryLength;
            }
        }
    }

    function renderDeepSearchResults(payload, query) {
        const items = Array.isArray(payload.items) ? payload.items : [];
        galleryGrid.innerHTML = '';

        if (items.length === 0) {
            galleryGrid.innerHTML = createEmptyStateMarkup('No matches found.');
        }
        else {
            items.forEach((item) => {
                galleryGrid.appendChild(createSingleCard(item));
            });
        }

        setInitialPagerVisible(false);
        renderDynamicPager(payload, query);

        headline.textContent = `'Single' - ${payload.totalMatches} deep match(es)`;
        filterStatus.textContent = `Showing page ${payload.page} of ${payload.totalPages} (${payload.totalMatches} total deep match(es)).`;

        if (payload.resultCapReached || payload.searchWasTruncated) {
            setWarning(`Showing up to ${payload.resultCap} matches for performance reasons. Refine your query for narrower results.`);
        }
        else {
            setWarning('');
        }
    }

    function renderDynamicPager(payload, query) {
        removeDynamicPager();

        dynamicPagerRow = renderPager({
            ariaLabel: 'Single deep search pages',
            totalItems: payload.totalMatches,
            pageSize: payload.pageSize ?? configuredPageSize,
            currentOffset: (Math.max(1, payload.page) - 1) * (payload.pageSize ?? configuredPageSize),
            windowSize: Math.max(1, window.webGalleryDisplay?.paginationWindowSize ?? 7),
            buildPageUrl: (pageNumber) => `?singleSearchPage=${pageNumber}`,
        });

        if (!dynamicPagerRow) {
            return;
        }

        dynamicPagerRow.addEventListener('click', (event) => {
            const link = event.target.closest('a.page-link');
            if (!link) {
                return;
            }

            event.preventDefault();
            const parentItem = link.closest('li.page-item');
            if (parentItem?.classList.contains('disabled') || parentItem?.classList.contains('active')) {
                return;
            }

            const url = new URL(link.href, window.location.origin);
            const targetPage = Number.parseInt(url.searchParams.get('singleSearchPage') ?? '', 10);
            if (!Number.isFinite(targetPage) || targetPage <= 0) {
                return;
            }

            triggerDeepSearch(targetPage);
        });

        photosContainer?.appendChild(dynamicPagerRow);
    }

    function createSingleCard(item) {
        const mediaType = (item.mediaType ?? '').toLowerCase();
        const normalizedAppPath = normalizePath(item.appPath ?? '');
        const appPathBase64 = getBase64Utf8(normalizedAppPath);
        const card = document.createElement('div');
        card.classList.add('col-6', 'col-md-6', 'col-lg-3');
        card.setAttribute('data-single-item', 'true');
        card.setAttribute('data-aos', 'fade-up');

        if (mediaType === 'video') {
            const thumbPath = getThumbPath(normalizedAppPath);
            const thumbBase64 = getBase64Utf8(thumbPath);
            const videoUri = `/files/video/${appPathBase64}`;

            card.innerHTML = [
                `<a href="${videoUri}" class="d-block photo-item" data-name="${escapeHtml(item.name ?? '')}" data-album="${escapeHtml(item.albumName ?? '')}" data-tags="${escapeHtml(item.tagSearchText ?? '')}" data-fancybox="gallery" data-type="video" data-caption="<div data-current-image='${escapeHtml(item.id ?? '')}'><a href='/Bio/id/${escapeHtml(item.id ?? '')}'>${escapeHtml(item.id ?? '')}</a></div>" data-video-autoplay="false">`,
                `<img src="/files/image/${thumbBase64}" alt="Video thumbnail" class="img-fluid">`,
                '</a>',
                '<div class="play-icon-overlay">',
                '<svg width="32" height="32" viewBox="0 0 32 32" fill="white" xmlns="http://www.w3.org/2000/svg">',
                '<circle cx="16" cy="16" r="16" fill="rgba(0,0,0,0.4)"/>',
                '<polygon points="12,9 24,16 12,23" fill="white"/>',
                '</svg>',
                '</div>'
            ].join('');

            return card;
        }

        const imageUri = `/files/image/${appPathBase64}`;
        card.innerHTML = [
            `<a href="${imageUri}" class="d-block photo-item" data-name="${escapeHtml(item.name ?? '')}" data-album="${escapeHtml(item.albumName ?? '')}" data-tags="${escapeHtml(item.tagSearchText ?? '')}" data-fancybox="gallery" data-type="image" data-caption="<div data-current-image='${escapeHtml(item.id ?? '')}'><a href='/Bio/id/${escapeHtml(item.id ?? '')}'>${escapeHtml(item.id ?? '')}</a></div>">`,
            `<img src="${imageUri}" alt="Image" class="img-fluid">`,
            '<div class="photo-text-more"><span class="icon icon-search"></span></div>',
            '</a>'
        ].join('');

        return card;
    }

    function createEmptyStateMarkup(message) {
        return `<div class="col-12 text-center py-5"><p class="text-white-50 mb-0">${message}</p></div>`;
    }

    function getThumbPath(appPath) {
        const normalized = normalizePath(appPath);
        const slashIndex = normalized.lastIndexOf('/');
        if (slashIndex <= 0) {
            return normalized;
        }

        const album = normalized.slice(0, slashIndex);
        const fileName = normalized.slice(slashIndex + 1);
        const fileNameNoExtension = fileName.replace(/\.[^/.]+$/, '');
        return `${album}/thumbs/${fileNameNoExtension}.jpg`;
    }

    function normalizePath(pathValue) {
        return (pathValue ?? '').replace(/\\/g, '/');
    }

    function getBase64Utf8(value) {
        const bytes = new TextEncoder().encode(value);
        let binary = '';
        bytes.forEach((byte) => {
            binary += String.fromCharCode(byte);
        });

        return window.btoa(binary);
    }

    function escapeHtml(value) {
        return String(value)
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#39;');
    }

    function debounce(fn, delayMs) {
        let timeoutHandle = null;
        return (...args) => {
            if (timeoutHandle !== null) {
                clearTimeout(timeoutHandle);
            }

            timeoutHandle = setTimeout(() => {
                timeoutHandle = null;
                fn(...args);
            }, delayMs);
        };
    }
}
