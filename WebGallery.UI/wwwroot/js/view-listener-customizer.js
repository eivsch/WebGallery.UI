function updateInput(values) {
    var valuesInt = Math.floor(values);

    var input = $('[data-number-pics]');
    input.val(valuesInt);
};

function initSlider() {
    if ($('[my-slider]').length > 0) {
        var slider = $('[my-slider]')[0];

        noUiSlider.create(slider, {
            start: [12],
            step: 2,
            connect: 'lower',
            range: {
                'min': 0,
                'max': 48
            },
            padding: [2, 0]
        });

        slider.noUiSlider.on('slide', updateInput);
    }
};

jQuery(document).ready(function ($) {
    initSlider();
});