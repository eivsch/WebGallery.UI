function handleSearchFormSubmit(event) {
    event.preventDefault();

    const albumsSelect = document.getElementById('albumsInput');
    const selectedAlbums = Array.from(albumsSelect.selectedOptions).map(option => option.value).join(',');
    const tagsSelect = document.getElementById('tagsInput');
    const selectedTags = Array.from(tagsSelect.selectedOptions).map(option => option.value).join(',');

    const fileExtensions = document.getElementById('fileExtensionsInput').value;
    const mediaNameContains = document.getElementById('mediaNameContainsInput').value;
    const allTagsMustMatch = document.getElementById('allTagsMustMatch').checked;
    const shuffle = document.getElementById('shuffle').checked;

    const queryParams = new URLSearchParams();
    if (selectedAlbums) queryParams.append('albums', selectedAlbums);
    if (selectedTags) queryParams.append('tags', selectedTags);
    if (fileExtensions) queryParams.append('fileExtensions', fileExtensions);
    if (mediaNameContains) queryParams.append('mediaNameContains', mediaNameContains);
    if (allTagsMustMatch) queryParams.append('allTagsMustMatch', 'true');
    if (shuffle) queryParams.append('shuffle', 'true');

    window.location.href = '/single/search?' + queryParams.toString();
    return false;
}

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

    $('[data-select-albums]').select2({
        placeholder: "Filter albums...",
    });

    $('[data-select-tags]').select2({
        placeholder: "Filter tags...",
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const saveBtn = document.getElementById('saveSearchBtn');
    const form = document.getElementById('searchForm');
    const status = document.getElementById('saveSearchStatus');
    const dropdown = document.getElementById('savedSearchesDropdown');
    const deleteBtn = document.getElementById('deleteSavedSearchBtn');
    const albumsInput = document.getElementById('albumsInput');
    const tagsInput = document.getElementById('tagsInput');

    function setAlbumsFromCsv(albumsCsv) {
        if (!albumsInput) return;

        const albums = (albumsCsv || '')
            .split(',')
            .map(function (album) { return album.trim(); })
            .filter(function (album) { return album.length > 0; });

        Array.from(albumsInput.options).forEach(function (option) {
            option.selected = albums.includes(option.value);
        });

        if (window.jQuery && jQuery.fn && jQuery.fn.select2) {
            jQuery(albumsInput).trigger('change');
        }
    }

    function getSelectedAlbumsAsCsv() {
        if (!albumsInput) return '';
        return Array.from(albumsInput.selectedOptions)
            .map(function (option) { return option.value; })
            .join(',');
    }

    function setTagsFromCsv(tagsCsv) {
        if (!tagsInput) return;

        const tags = (tagsCsv || '')
            .split(',')
            .map(function (tag) { return tag.trim(); })
            .filter(function (tag) { return tag.length > 0; });

        Array.from(tagsInput.options).forEach(function (option) {
            option.selected = tags.includes(option.value);
        });

        if (window.jQuery && jQuery.fn && jQuery.fn.select2) {
            jQuery(tagsInput).trigger('change');
        }
    }

    function getSelectedTagsAsCsv() {
        if (!tagsInput) return '';
        return Array.from(tagsInput.selectedOptions)
            .map(function (option) { return option.value; })
            .join(',');
    }

    // Autofill form when a saved search is selected
    if (dropdown) {
        dropdown.addEventListener('change', function () {
            const selected = dropdown.options[dropdown.selectedIndex];
            if (!selected || !selected.value) {
                // Clear form if no selection
                form.reset();
                setAlbumsFromCsv('');
                setTagsFromCsv('');
                return;
            }
            setAlbumsFromCsv(selected.getAttribute('data-albums') || '');
            setTagsFromCsv(selected.getAttribute('data-tags') || '');
            document.getElementById('fileExtensionsInput').value = selected.getAttribute('data-fileextensions') || '';
            document.getElementById('mediaNameContainsInput').value = selected.getAttribute('data-medianamecontains') || '';
            document.getElementById('allTagsMustMatch').checked = selected.getAttribute('data-alltagsmustmatch') === "True" || selected.getAttribute('data-alltagsmustmatch') === "true";
            // Add maxsize if you have a field for it
        });
    }

    // Save or update search
    if (saveBtn && form) {
        saveBtn.addEventListener('click', async function () {
            let searchName = dropdown && dropdown.value ? dropdown.value : null;
            // If no search selected, prompt for a new name
            if (!searchName) {
                searchName = prompt("Enter a name for this search:");
                if (!searchName) return;
            } else {
                // Ask if user wants to overwrite or save as new
                const overwrite = confirm("Overwrite the selected saved search '" + searchName + "'? Click Cancel to save as a new search.");
                if (!overwrite) {
                    searchName = prompt("Enter a name for this search:");
                    if (!searchName) return;
                }
            }

            const data = {
                Albums: getSelectedAlbumsAsCsv(),
                Tags: getSelectedTagsAsCsv(),
                FileExtensions: document.getElementById('fileExtensionsInput').value || "",
                MediaNameContains: document.getElementById('mediaNameContainsInput').value || "",
                AllTagsMustMatch: document.getElementById('allTagsMustMatch').checked,
                SearchName: searchName,
                MaxSize: null // Add if you have a field for this
            };

            try {
                const response = await fetch('/customizer/save-search', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify(data)
                });
                if (response.ok) {
                    status.textContent = "Search saved!";
                    status.style.display = "inline";
                    setTimeout(() => status.style.display = "none", 2000);
                    // Optionally, reload the page or update the dropdown
                    setTimeout(() => location.reload(), 1000);
                } else {
                    status.textContent = "Failed to save search.";
                    status.style.display = "inline";
                }
            } catch (e) {
                status.textContent = "Error saving search.";
                status.style.display = "inline";
            }
        });
    }

    // Delete saved search
    if (deleteBtn && dropdown) {
        deleteBtn.addEventListener('click', async function () {
            const searchName = dropdown.value;
            if (!searchName) return;
            if (!confirm("Delete saved search '" + searchName + "'?")) return;
            try {
                const response = await fetch('/customizer/delete-saved-search?searchName=' + encodeURIComponent(searchName), {
                    method: 'DELETE'
                });
                if (response.ok) {
                    status.textContent = "Search deleted!";
                    status.style.display = "inline";
                    setTimeout(() => status.style.display = "none", 2000);
                    setTimeout(() => location.reload(), 1000);
                } else {
                    status.textContent = "Failed to delete search.";
                    status.style.display = "inline";
                }
            } catch (e) {
                status.textContent = "Error deleting search.";
                status.style.display = "inline";
            }
        });
    }
});