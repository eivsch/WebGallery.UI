var album = "placeholder";

$.fancybox.defaults.btnTpl.like = '<button data-fancybox-like class="fancybox-button fancybox-button--like" title="Like">L</button>';
$.fancybox.defaults.buttons = [
    "like",
    "zoom",
    "fullScreen",
    "thumbs",
    "close"
],

$('body').on('click', '[data-fancybox-like]', function (e) {
    var currentImage = $('[data-current-image]')[0];
    var currentImageId = $(currentImage).data('current-image');

    $.post(
        "/albums/" + album + "/" + currentImageId + "/add-like",
        null,
        function () {
            var btn = $(e.currentTarget);

            btn.addClass('fancybox-like__pressed');
        }
    );
});

$(document).on('afterShow.fb', function (e, instance, slide) {
    var likeBtn = $('[data-fancybox-like]');
    if (likeBtn.hasClass('fancybox-like__pressed')) {
        likeBtn.removeClass('fancybox-like__pressed');
    }
});