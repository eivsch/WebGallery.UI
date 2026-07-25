const headline = document.querySelector('h2.text-white');
headline.textContent = "Tags";

const tagsContainer = document.querySelector('div.row.align-items-stretch');
const pageSize = 48;
const currentPage = getCurrentPage();

const albums = await getAlbums();
const tags = await getTags();
const totalPages = Math.max(1, Math.ceil(tags.length / pageSize));
const clampedPage = Math.min(currentPage, totalPages);
const pageStart = (clampedPage - 1) * pageSize;
const pageEnd = pageStart + pageSize;
const pageTags = tags.slice(pageStart, pageEnd);

headline.textContent = `Tags (${tags.length})`;

for (const tag of pageTags) {
    const albumWithTag = findAlbumWithTag(tag);
    if (!albumWithTag) {
        continue;
    }

    const mediaWithTag = await getThumbnailImageFromAlbum(tag.tagName, albumWithTag);
    if (!mediaWithTag) {
        continue;
    }

    const thumbnailElem = createThumbnailElem(mediaWithTag, albumWithTag, tag);
    tagsContainer.appendChild(thumbnailElem);
}

if (tags.length > pageSize) {
    renderPagination(totalPages, clampedPage);
}

function findAlbumWithTag(tag) {
    var tagAlbums = [];
    albums.forEach((alb) => {
        const foundTag = alb.tags.find((el) => el.tagName == tag.tagName);
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

function renderPagination(totalPages, currentPageNumber) {
    const photosContainer = document.querySelector(".container-fluid.photos");
    if (!photosContainer) {
        return;
    }

    const row = document.createElement("div");
    row.classList.add("row", "justify-content-center", "mt-4", "mb-3", "pager-row");

    const col = document.createElement("div");
    col.classList.add("col-auto", "pager-col");

    const nav = document.createElement("nav");
    nav.classList.add("pager-nav");
    nav.setAttribute("aria-label", "Tag pages");

    const list = document.createElement("ul");
    list.classList.add("pagination", "mb-0", "pager-list");

    list.appendChild(createPageItem("First", 1, currentPageNumber === 1));
    list.appendChild(createPageItem("Previous", Math.max(1, currentPageNumber - 1), currentPageNumber === 1));

    const windowSize = 7;
    const halfWindow = Math.floor(windowSize / 2);
    let firstPage = Math.max(1, currentPageNumber - halfWindow);
    let lastPage = Math.min(totalPages, firstPage + windowSize - 1);
    firstPage = Math.max(1, lastPage - windowSize + 1);

    for (let page = firstPage; page <= lastPage; page++) {
        list.appendChild(createPageItem(page.toString(), page, false, page === currentPageNumber));
    }

    list.appendChild(createPageItem("Next", Math.min(totalPages, currentPageNumber + 1), currentPageNumber === totalPages));
    list.appendChild(createPageItem("Last", totalPages, currentPageNumber === totalPages));

    nav.appendChild(list);
    col.appendChild(nav);
    row.appendChild(col);
    photosContainer.appendChild(row);
}

function createPageItem(label, pageNumber, disabled, active = false) {
    const li = document.createElement("li");
    li.classList.add("page-item");

    if (disabled) {
        li.classList.add("disabled");
    }

    if (active) {
        li.classList.add("active");
    }

    const a = document.createElement("a");
    a.classList.add("page-link");
    a.textContent = label;

    if (disabled || active) {
        a.href = "#";
        a.setAttribute("tabindex", "-1");
        a.setAttribute("aria-disabled", "true");
    } else {
        a.href = buildPageUrl(pageNumber);
    }

    li.appendChild(a);
    return li;
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
            const foundTag = mediaItem.tags.find((mediaTag) => mediaTag.tagName == tagName);
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
