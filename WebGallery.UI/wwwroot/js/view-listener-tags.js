const headline = document.querySelector('h2.text-white');
headline.textContent = "Tags: ";

const tagsContainer = document.querySelector('div.row.align-items-stretch');

const albums = await getAlbums();
const tags = await getTags();
for (const i in tags) {
    const albumWithTag = findAlbumWithTag(tags[i]);
    const mediaWithTag = await getThumbnailImageFromAlbum(tags[i].tagName, albumWithTag);
    const thumbnailElem = createThumbnailElem(mediaWithTag, albumWithTag, tags[i]);
    tagsContainer.appendChild(thumbnailElem);
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
        const mediaUrib64 = window.btoa(mediaUri);

        const vid = document.createElement("video");
        vid.classList.add("img-fluid");
        vid.setAttribute("autoplay");
        vid.setAttribute("loop");
        vid.setAttribute("muted");

        const srcEl = document.createElement("source");
        srcEl.src = uriPrefix + mediaUrib64;
        srcEl.setAttribute("type", "video/mp4");

        vid.appendChild(srcEl);
        a.appendChild(vid);
    }
    else {
        const uriPrefix = "/files/image/";
        const mediaUri = album.albumName + "/" + mediaWithTag.name;
        const mediaUrib64 = window.btoa(mediaUri);

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
