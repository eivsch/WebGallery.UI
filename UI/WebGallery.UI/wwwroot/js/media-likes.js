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
    $.fancybox.defaults.btnTpl.sourceVideo = '<button type="button" data-fancybox-source-video class="fancybox-button fancybox-button--source-video" title="Open source video" aria-label="Open source video" hidden><i class="fas fa-video" aria-hidden="true"></i></button>';
    $.fancybox.defaults.buttons = [
        'like',
        'sourceVideo',
        'zoom',
        'fullScreen',
        'thumbs',
        'close'
    ];

    $('body').on('click', '[data-fancybox-like]', function (e) {
        var media = getMedia();
        addMediaLike($(e.currentTarget), media.album, media.id);
    });

    $('body').on('click', '[data-fancybox-source-video]', function (e) {
        var sourceUrl = $(e.currentTarget).attr('data-source-url');
        if (sourceUrl) {
            window.location.assign(sourceUrl);
        }
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
        var sourceButton = instance.$refs.toolbar.find('[data-fancybox-source-video]');
        var original = instance.current && instance.current.opts.$orig;
        var album = original && original.attr('data-screencap-album');
        var name = original && original.attr('data-screencap-name');

        if (!album || !name) {
            sourceButton.prop('hidden', true).removeAttr('data-source-url');
            return;
        }

        var sourceUrl = new URL('/Bio/source-video', window.location.origin);
        sourceUrl.searchParams.set('album', album);
        sourceUrl.searchParams.set('name', name);
        sourceButton.attr('data-source-url', sourceUrl.toString()).prop('hidden', false);
    });
}
