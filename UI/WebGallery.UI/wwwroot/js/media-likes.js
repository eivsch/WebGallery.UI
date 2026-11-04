function addMediaLike(button, album, mediaId) {
    return $.post(
        '/albums/' + encodeURIComponent(album) + '/' + encodeURIComponent(mediaId) + '/add-like',
        null,
        function () {
            button.addClass('fancybox-like__pressed');
        }
    ).fail(function () {
        window.alert('Unable to like this item. Please try again.');
    });
}

function setupGalleryLikes(getMedia, enableShortcut) {
    var title = enableShortcut ? 'Like (L)' : 'Like';
    $.fancybox.defaults.btnTpl.like = '<button data-fancybox-like class="fancybox-button fancybox-button--like" title="' + title + '">L</button>';
    $.fancybox.defaults.buttons = [
        'like',
        'zoom',
        'fullScreen',
        'thumbs',
        'close'
    ];

    $('body').on('click', '[data-fancybox-like]', function (e) {
        var media = getMedia();
        addMediaLike($(e.currentTarget), media.album, media.id);
    });

    if (enableShortcut) {
        $(document).on('afterKeydown.fb', function (e, instance, slide, keypress, keycode) {
            if (keycode !== 76 || slide.type !== 'image' ||
                keypress.ctrlKey || keypress.altKey || keypress.shiftKey || keypress.metaKey ||
                keypress.target.isContentEditable ||
                $(keypress.target).is('input,textarea,select,video,audio')) {
                return;
            }

            keypress.preventDefault();
            instance.$refs.toolbar.find('[data-fancybox-like]').trigger('click');
        });
    }

    $(document).on('afterShow.fb', function (e, instance) {
        instance.$refs.toolbar.find('[data-fancybox-like]').removeClass('fancybox-like__pressed');
    });
}
