function doSomething(values) {
    var valuesInt = Math.floor(values);

    var input = $('[data-number-pics]');
    input.val(valuesInt);

};

function initSlider() {
    var slider = $('[my-slider]')[0];

    noUiSlider.create(slider, {
        start: [2],
        step: 2,
        connect: 'lower',
        range: {
            'min': 2,
            'max': 48
        },
    });

    slider.noUiSlider.on('slide', doSomething);
};