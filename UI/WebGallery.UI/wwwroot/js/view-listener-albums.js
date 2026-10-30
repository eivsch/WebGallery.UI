const headline = document.querySelector('h2.text-white');
const albumsContainer = document.querySelector('div.row.align-items-stretch');
const filterInput = document.querySelector('#album-filter-input');
const filterStatus = document.querySelector('#album-filter-status');
const pagerRow = document.querySelector('.pager-row');

if (headline && albumsContainer && filterInput && filterStatus) {
    const initialHeadlineText = headline.textContent ?? '';
    const initialGridHtml = albumsContainer.innerHTML;
    const initialPagerDisplay = pagerRow?.style.display ?? '';
    const albumCardPromiseCache = new Map();
    let filterRequestId = 0;

    filterInput.addEventListener('input', debounce(async () => {
        const requestId = ++filterRequestId;
        const filterValue = (filterInput.value ?? '').trim();

        if (!filterValue) {
            if (requestId !== filterRequestId) {
                return;
            }

            filterStatus.textContent = '';
            headline.textContent = initialHeadlineText;
            setPaginationVisibility(true);
            albumsContainer.innerHTML = initialGridHtml;
            return;
        }

        setPaginationVisibility(false);
        filterStatus.textContent = 'Searching all albums...';
        headline.textContent = `Albums (${filterValue})`;

        const matches = await searchAlbums(filterValue);
        if (requestId !== filterRequestId) {
            return;
        }

        if (matches.length === 0) {
            filterStatus.textContent = 'No albums found.';
            headline.textContent = 'Albums (0)';
            albumsContainer.innerHTML = createEmptyStateMarkup('No albums found.');
            return;
        }

        filterStatus.textContent = `Showing ${matches.length} match(es) from all albums.`;
        headline.textContent = `Albums (${matches.length})`;
        await renderAlbums(matches);
    }, 220));

    function setPaginationVisibility(visible) {
        if (!pagerRow) {
            return;
        }

        pagerRow.style.display = visible ? initialPagerDisplay : 'none';
    }

    async function renderAlbums(albumList) {
        albumsContainer.innerHTML = '';

        const cardPromises = albumList.map((album) => getAlbumCardElement(album));
        const cards = await Promise.all(cardPromises);
        cards.filter(Boolean).forEach((card) => albumsContainer.appendChild(card));

        if (albumsContainer.childElementCount === 0) {
            albumsContainer.innerHTML = createEmptyStateMarkup('No albums found.');
        }
    }

    async function getAlbumCardElement(album) {
        const normalizedAlbumName = normalizeValue(album.albumName);
        if (!normalizedAlbumName) {
            return null;
        }

        if (!albumCardPromiseCache.has(normalizedAlbumName)) {
            albumCardPromiseCache.set(normalizedAlbumName, buildAlbumCardElement(album));
        }

        const card = await albumCardPromiseCache.get(normalizedAlbumName);
        if (!card) {
            return null;
        }

        return card.cloneNode(true);
    }

    async function buildAlbumCardElement(album) {
        const coverItem = await getAlbumCoverItem(album.albumName);
        if (!coverItem) {
            return null;
        }

        return createAlbumCard(album, coverItem);
    }

    async function searchAlbums(filterValue) {
        const response = await fetch(`/data/albums/search?q=${encodeURIComponent(filterValue)}`);
        if (!response.ok) {
            return [];
        }

        return await response.json();
    }

    async function getAlbumCoverItem(albumName) {
        const response = await fetch(`/data/albums/${encodeURIComponent(albumName)}?from=0&itemCount=1`);
        if (!response.ok) {
            return null;
        }

        const albumContent = await response.json();
        if (!albumContent?.items?.length) {
            return null;
        }

        return albumContent.items[0];
    }

    function createAlbumCard(album, coverItem) {
        const col = document.createElement('div');
        col.classList.add('col-6', 'col-md-6', 'col-lg-3');
        col.setAttribute('data-aos', 'fade-up');

        const link = document.createElement('a');
        link.classList.add('d-block', 'photo-item');
        link.href = `/Albums/${encodeURIComponent(album.albumName)}`;

        const mediaUri = getBase64Utf8(`${album.albumName}/${coverItem.name}`);
        if (isVideoFile(coverItem.name)) {
            const video = document.createElement('video');
            video.classList.add('img-fluid');
            video.setAttribute('muted', '');
            video.setAttribute('loop', '');
            video.setAttribute('preload', 'metadata');

            const source = document.createElement('source');
            source.src = `/files/video/${mediaUri}`;
            source.type = 'video/mp4';
            video.appendChild(source);
            link.appendChild(video);
        }
        else {
            const img = document.createElement('img');
            img.classList.add('img-fluid');
            img.alt = 'Image';
            img.src = `/files/image/${mediaUri}`;
            link.appendChild(img);
        }

        const photoText = document.createElement('div');
        photoText.classList.add('photo-text-more');

        const heading = document.createElement('h3');
        heading.classList.add('heading');
        heading.textContent = album.albumName;

        const meta = document.createElement('span');
        meta.classList.add('meta');
        meta.textContent = `${album.totalCount} Photos`;

        photoText.appendChild(heading);
        photoText.appendChild(meta);
        link.appendChild(photoText);
        col.appendChild(link);

        return col;
    }

    function createEmptyStateMarkup(message) {
        return `<div class="col-12 text-center py-5"><p class="text-white-50 mb-0">${message}</p></div>`;
    }

    function isVideoFile(fileName) {
        return /\.(mp4|webm|ogg)$/i.test(fileName ?? '');
    }

    function normalizeValue(value) {
        return (value ?? '').trim().toLowerCase();
    }

    function getBase64Utf8(value) {
        const bytes = new TextEncoder().encode(value);
        let binary = '';
        bytes.forEach((byte) => {
            binary += String.fromCharCode(byte);
        });

        return window.btoa(binary);
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