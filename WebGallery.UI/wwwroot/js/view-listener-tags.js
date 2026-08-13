import { renderPager } from './pager.js';

const headline = document.querySelector('h2.text-white');
headline.textContent = "Tags";

const tagsContainer = document.querySelector('div.row.align-items-stretch');
const filterInput = document.querySelector('#tag-filter-input');
const filterStatus = document.querySelector('#tag-filter-status');
const pageSize = window.webGalleryDisplay?.pageSize ?? 48;
const paginationWindowSize = Math.max(1, window.webGalleryDisplay?.paginationWindowSize ?? 7);
const currentPage = getCurrentPage();
const tagCardPromiseCache = new Map();
let paginationRow = null;
let filterRequestId = 0;

const albums = await getAlbums();
const tags = await getTags();
const totalPages = Math.max(1, Math.ceil(tags.length / pageSize));
const clampedPage = Math.min(currentPage, totalPages);
const pageStart = (clampedPage - 1) * pageSize;
const pageEnd = pageStart + pageSize;
const pageTags = tags.slice(pageStart, pageEnd);

headline.textContent = `Tags (${tags.length})`;

await renderTags(pageTags);

if (tags.length > pageSize) {
    paginationRow = renderPager({
        ariaLabel: "Tag pages",
        totalItems: tags.length,
        pageSize,
        currentOffset: pageStart,
        windowSize: paginationWindowSize,
        buildPageUrl,
    });

    if (paginationRow) {
        const photosContainer = document.querySelector(".container-fluid.photos");
        photosContainer?.appendChild(paginationRow);
    }
}

if (filterInput) {
    filterInput.addEventListener('input', debounce(async () => {
        const requestId = ++filterRequestId;
        const rawFilter = filterInput.value ?? "";
        const filterValue = rawFilter.trim();

        if (!filterValue) {
            if (requestId !== filterRequestId) {
                return;
            }

            filterStatus.textContent = "";
            setPaginationVisibility(true);
            await renderTags(pageTags);
            return;
        }

        setPaginationVisibility(false);
        filterStatus.textContent = "Searching all tags...";

        const fallbackMatches = await searchTags(filterValue);
        if (requestId !== filterRequestId) {
            return;
        }

        if (fallbackMatches.length === 0) {
            filterStatus.textContent = "No tags found.";
            clearTags();
            return;
        }

        filterStatus.textContent = `Showing ${fallbackMatches.length} match(es) from all pages.`;
        await renderTags(fallbackMatches);
    }, 220));
}

function normalizeValue(value) {
    return (value ?? "").trim().toLowerCase();
}

async function renderTags(tagList) {
    clearTags();

    const cardPromises = tagList.map((tag) => getTagCardElement(tag));
    const cards = await Promise.all(cardPromises);
    cards.filter(Boolean).forEach((card) => tagsContainer.appendChild(card));
}

function clearTags() {
    tagsContainer.innerHTML = "";
}

function setPaginationVisibility(visible) {
    if (!paginationRow) {
        return;
    }

    paginationRow.style.display = visible ? "" : "none";
}

async function getTagCardElement(tag) {
    const normalizedTagName = normalizeValue(tag.tagName);
    if (!normalizedTagName) {
        return null;
    }

    if (!tagCardPromiseCache.has(normalizedTagName)) {
        tagCardPromiseCache.set(normalizedTagName, buildTagCardElement(tag));
    }

    const card = await tagCardPromiseCache.get(normalizedTagName);
    if (!card) {
        return null;
    }

    return card.cloneNode(true);
}

async function buildTagCardElement(tag) {
    const albumWithTag = findAlbumWithTag(tag);
    if (!albumWithTag) {
        return null;
    }

    const mediaWithTag = await getThumbnailImageFromAlbum(tag.tagName, albumWithTag);
    if (!mediaWithTag) {
        return null;
    }

    return createThumbnailElem(mediaWithTag, albumWithTag, tag);
}

async function searchTags(filterValue) {
    const response = await fetch(`data/tags/search?q=${encodeURIComponent(filterValue)}`);
    if (!response.ok) {
        return [];
    }

    return await response.json();
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

function findAlbumWithTag(tag) {
    var tagAlbums = [];
    albums.forEach((alb) => {
        const foundTag = alb.tags.find((el) => normalizeValue(el.tagName) === normalizeValue(tag.tagName));
        if (foundTag) {
            tagAlbums.push(alb);
        }
    });

    return tagAlbums[0];    // TODO: ranomize?
}

function createThumbnailElem(mediaWithTag, album, tag) {
    const col = document.createElement("div");
    col.classList.add("col-lg-3");
    col.classList.add("col-sm-4");
    col.setAttribute("data-aos", "fade-up");
    const a = document.createElement("a");
    a.href = "single/search?tags=" + tag.tagName;
    a.classList.add("d-block");
    a.classList.add("photo-item");

    if (mediaWithTag.name.endsWith(".mp4")) {
        const uriPrefix = "/files/video/";
        const mediaUri = album.albumName + "/" + mediaWithTag.name;
        const mediaUrib64 = getBase64Utf8(mediaUri);
        const thumbnailUrib64 = getThumbsBase64(album.albumName, mediaWithTag.name);

        const vid = document.createElement("video");
        vid.classList.add("img-fluid");
        vid.setAttribute("muted", true);
        vid.setAttribute("preload", "none");
        vid.setAttribute("poster", "/files/image/" + thumbnailUrib64);

        const srcEl = document.createElement("source");
        srcEl.src = uriPrefix + mediaUrib64;
        srcEl.setAttribute("type", "video/mp4");

        vid.appendChild(srcEl);
        a.appendChild(vid);
    }
    else {
        const uriPrefix = "/files/image/";
        const mediaUri = album.albumName + "/" + mediaWithTag.name;
        const mediaUrib64 = getBase64Utf8(mediaUri);

        const img = document.createElement("img");
        img.src = uriPrefix + mediaUrib64;
        img.alt = "Image";
        img.classList.add("img-fluid");

        a.appendChild(img);
    }

    const heading1 = document.createElement("div");
    heading1.classList.add("photo-text-more");
    const h3 = document.createElement("h3");
    h3.classList.add("heading");
    h3.textContent = tag.tagName;
    const spanEl = document.createElement("span");
    spanEl.classList.add("meta");
    spanEl.textContent = tag.count + " items";

    heading1.appendChild(h3);
    heading1.appendChild(spanEl);
    a.appendChild(heading1);

    col.appendChild(a);

    return col;
}

function getThumbsBase64(albumName, fileName) {
    const fileNameNoExt = fileName.replace(/\.[^/.]+$/, "");
    const thumbsName = fileNameNoExt + ".jpg";
    const thumbsPath = albumName + "/thumbs/" + thumbsName;
    return getBase64Utf8(thumbsPath);
}

function getBase64Utf8(value) {
    const bytes = new TextEncoder().encode(value);
    let binary = "";
    bytes.forEach((b) => {
        binary += String.fromCharCode(b);
    });

    return window.btoa(binary);
}

function getCurrentPage() {
    const params = new URLSearchParams(window.location.search);
    const page = parseInt(params.get("page"), 10);
    return Number.isFinite(page) && page > 0 ? page : 1;
}

function buildPageUrl(pageNumber) {
    const url = new URL(window.location.href);
    url.searchParams.set("page", pageNumber.toString());
    return url.pathname + url.search;
}

async function getAlbums() {
    const response = await fetch('data/albums');
    const myJson = await response.json();
    return myJson;
}

async function getTags() {
    const response = await fetch('data/tags');
    const myJson = await response.json();
    return myJson;
}

async function getThumbnailImageFromAlbum(tagName, album) {
    var from = 0;
    var scrollSize = 50;
    var maxItemsToRead = scrollSize * 100;
    while (from < maxItemsToRead) {
        const response = await fetch('data/albums/' + album.albumName + '?from=' + from + '&itemCount=' + scrollSize);
        const albumContent = await response.json();
        var itemsWithTag = [];
        albumContent.items.forEach((mediaItem) => {
            const foundTag = mediaItem.tags.find((mediaTag) => normalizeValue(mediaTag.tagName) === normalizeValue(tagName));
            if (foundTag) {
                itemsWithTag.push(mediaItem);
            }
        });

        if (itemsWithTag.length > 0) {
            return itemsWithTag[0]; // TODO: randomize?
        }

        from += scrollSize-1;
        if (from >= albumContent.totalCount) return null;
    }
}
