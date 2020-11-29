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

$('input:radio[name="RadioTagmodeOption"]').change(
    function () {
        if ($(this).is(':checked') && $(this).val() == 'custom') {
            $('[data-tag-manager]').show();
        }
        else {
            $('[data-tag-manager]').hide();
        }
    }
);

jQuery(document).ready(function ($) {
    initSlider();

    $('[data-select-tags]').select2({
        placeholder: "Search tags...",
    });
});