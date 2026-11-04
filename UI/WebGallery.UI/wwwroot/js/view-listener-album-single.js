var album = "placeholder";

setupGalleryLikes(function () {
    return {
        album: album,
        id: $('[data-current-image]').first().data('current-image')
    };
}, true);