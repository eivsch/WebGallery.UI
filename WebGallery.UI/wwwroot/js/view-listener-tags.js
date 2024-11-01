var container = document.querySelectorAll('main.main-content');
const albums = await getAlbums();
//const thmb = await getThumbnailImage("man", albums[0]);
//console.log(thmb);

const tags = await getTags();
for (const i in tags) {
    const albumWithTag = findAlbumWithTag(tags[i]);
    const thmb = await getThumbnailImage(tags[i].tagName, albumWithTag);
//    console.log(tags[i]);
//    console.log(albumWithTag);
    console.log(thmb);
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

function createThumbnailElem(thmb, album) {
    const col = document.createElement("div");
    col.classList.add("col-lg-3");
    col.classList.add("col-sm-4");
    col.setAttribute("data-aos", "fade-up");
    const a = document.createElement("a");
    a.href = "tbc";
    a.classList.add("d-block");
    a.classList.add("photo-item");

    if (thmb.endsWith(".mp4")) {
        const uriPrefix = "/files/video/";
        const mediaUri = uriPrefix + album.albumName + "/" + thmb.name;
        const mediaUrib64 = window.btoa(mediaUri);

        const vid = document.createElement("video");
        vid.classList.add("img-fluid");
        vid.setAttribute("autoplay");
        vid.setAttribute("loop");
        vid.setAttribute("muted");

        const srcEl = document.createElement("source");
        srcEl.src = mediaUrib64;
        srcEl.setAttribute("type", "video/mp4");
    }
    else {
        const uriPrefix = "/files/image/";
        const mediaUri = uriPrefix + album.albumName + "/" + thmb.name;
        const mediaUrib64 = window.btoa(mediaUri);

        const img = document.createElement("img");
        img.src = mediaUrib64;
        img.alt = "Image";
        img.classList.add("img-fluid");
    }

    const heading1 = document.createElement("div");
    heading1.classList.add("photo-text-more");
    const h3 = document.createElement("h3");
    h3.classList.add("heading");
    h3.textContent = "";
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

async function getThumbnailImage(tagName, album) {
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

        if (itemsWithTag) {
            return itemsWithTag[0]; // TODO: randomize?
        }

        from += scrollSize-1;
        if (from >= albumContent.totalCount) return null;
    }
}
