const BYTES_PER_MB = 1024 * 1024;

function megabytesToBytes(megabytes) {
    const parsed = parseFloat(megabytes);
    if (Number.isNaN(parsed)) return null;
    return Math.round(parsed * BYTES_PER_MB);
}

function bytesToMegabytes(bytes) {
    const parsed = parseFloat(bytes);
    if (Number.isNaN(parsed)) return '';
    return parsed / BYTES_PER_MB;
}

function handleSearchFormSubmit(event) {
    event.preventDefault();

    const albumsSelect = document.getElementById('albumsInput');
    const selectedAlbums = Array.from(albumsSelect.selectedOptions).map(option => option.value).join(',');
    const tagsSelect = document.getElementById('tagsInput');
    const selectedTags = Array.from(tagsSelect.selectedOptions).map(option => option.value).join(',');

    const fileExtensions = document.getElementById('fileExtensionsInput').value;
    const mediaNameContains = document.getElementById('mediaNameContainsInput').value;
    const maxSizeInput = document.getElementById('maxSizeInput');
    const minFileSizeInput = document.getElementById('minFileSizeInput');
    const maxFileSizeInput = document.getElementById('maxFileSizeInput');
    const allTagsMustMatch = document.getElementById('allTagsMustMatch').checked;
    const shuffle = document.getElementById('shuffle').checked;
    const createdAfter = document.getElementById('createdAfterInput').value;
    const createdBefore = document.getElementById('createdBeforeInput').value;

    const queryParams = new URLSearchParams();
    if (selectedAlbums) queryParams.append('albums', selectedAlbums);
    if (selectedTags) queryParams.append('tags', selectedTags);
    if (fileExtensions) queryParams.append('fileExtensions', fileExtensions);
    if (createdAfter) queryParams.append('createdAfter', createdAfter);
    if (createdBefore) queryParams.append('createdBefore', createdBefore);
    if (mediaNameContains) queryParams.append('mediaNameContains', mediaNameContains);
    if (maxSizeInput && maxSizeInput.value) queryParams.append('maxSize', maxSizeInput.value);
    if (minFileSizeInput && minFileSizeInput.value) queryParams.append('minFileSize', megabytesToBytes(minFileSizeInput.value));
    if (maxFileSizeInput && maxFileSizeInput.value) queryParams.append('maxFileSize', megabytesToBytes(maxFileSizeInput.value));
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
    const defaultSearchMaxSize = 200;
    const saveBtn = document.getElementById('saveSearchBtn');
    const form = document.getElementById('searchForm');
    const status = document.getElementById('saveSearchStatus');
    const dropdown = document.getElementById('savedSearchesDropdown');
    const deleteBtn = document.getElementById('deleteSavedSearchBtn');
    const albumsInput = document.getElementById('albumsInput');
    const tagsInput = document.getElementById('tagsInput');
    const albumMatchCount = document.getElementById('albumMatchCount');
    const tagMatchCount = document.getElementById('tagMatchCount');
    const maxSizeInput = document.getElementById('maxSizeInput');
    const maxSizeWarning = document.getElementById('maxSizeWarning');
    const relativeDateRangeButtons = document.querySelectorAll('[data-relative-date-range]');

    function formatLocalDate(date) {
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');

        return year + '-' + month + '-' + day;
    }

    function applyRelativeDateRange(days) {
        const createdAfterInput = document.getElementById('createdAfterInput');
        const createdBeforeInput = document.getElementById('createdBeforeInput');

        if (!createdAfterInput || !createdBeforeInput) return;

        const today = new Date();
        const startDate = new Date(today);
        startDate.setDate(today.getDate() - days);

        createdAfterInput.value = formatLocalDate(startDate);
        createdBeforeInput.value = '';
    }

    function normalizeMaxSizeInput() {
        if (!maxSizeInput) return;

        const totalItems = parseInt(maxSizeInput.getAttribute('data-total-items'), 10);
        const parsedValue = parseInt(maxSizeInput.value, 10);

        if (!Number.isNaN(parsedValue) && !Number.isNaN(totalItems) && parsedValue > totalItems) {
            maxSizeInput.value = totalItems.toString();
        }

        const normalizedValue = parseInt(maxSizeInput.value, 10);
        const shouldShowWarning = !Number.isNaN(normalizedValue) && normalizedValue > defaultSearchMaxSize;

        if (maxSizeWarning) {
            maxSizeWarning.style.display = shouldShowWarning ? 'block' : 'none';
        }
    }

    function getOptionCountsMap(selectElement) {
        if (!selectElement) return {};

        return Array.from(selectElement.options).reduce(function (acc, option) {
            const value = option.value;
            const count = parseInt(option.getAttribute('data-count'), 10);

            acc[value] = Number.isNaN(count) ? 0 : count;
            return acc;
        }, {});
    }

    const albumCountsByName = getOptionCountsMap(albumsInput);
    const tagCountsByName = getOptionCountsMap(tagsInput);

    function getSelectedValues(selectElement) {
        if (!selectElement) return [];

        if (window.jQuery) {
            const values = jQuery(selectElement).val();
            if (Array.isArray(values)) {
                return values;
            }
        }

        return Array.from(selectElement.selectedOptions || []).map(function (option) {
            return option.value;
        });
    }

    function sumSelectedCounts(selectElement, countsByName) {
        if (!selectElement) return 0;

        return getSelectedValues(selectElement).reduce(function (sum, value) {
            const count = countsByName[value] || 0;
            return sum + count;
        }, 0);
    }

    function updateDynamicMatchCount() {
        if (!albumMatchCount && !tagMatchCount) return;

        const totalAlbumItems = albumMatchCount ? (parseInt(albumMatchCount.getAttribute('data-total-items'), 10) || 0) : 0;
        const totalTagItems = tagMatchCount ? (parseInt(tagMatchCount.getAttribute('data-total-items'), 10) || 0) : 0;
        const selectedAlbumValues = getSelectedValues(albumsInput);
        const selectedTagValues = getSelectedValues(tagsInput);
        const selectedAlbumCount = selectedAlbumValues.length > 0
            ? sumSelectedCounts(albumsInput, albumCountsByName)
            : totalAlbumItems;
        const selectedTagCount = selectedTagValues.length > 0
            ? sumSelectedCounts(tagsInput, tagCountsByName)
            : totalTagItems;

        if (albumMatchCount) {
            albumMatchCount.textContent = 'Albums: ' + selectedAlbumCount + ' of ' + totalAlbumItems + ' items';
        }

        if (tagMatchCount) {
            tagMatchCount.textContent = 'Tags: ' + selectedTagCount + ' of ' + totalTagItems + ' items';
        }
    }

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

        updateDynamicMatchCount();
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

        updateDynamicMatchCount();
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
                updateDynamicMatchCount();
                return;
            }
            setAlbumsFromCsv(selected.getAttribute('data-albums') || '');
            setTagsFromCsv(selected.getAttribute('data-tags') || '');
            document.getElementById('fileExtensionsInput').value = selected.getAttribute('data-fileextensions') || '';
            document.getElementById('mediaNameContainsInput').value = selected.getAttribute('data-medianamecontains') || '';
            document.getElementById('minFileSizeInput').value = bytesToMegabytes(selected.getAttribute('data-minfilesize'));
            document.getElementById('maxFileSizeInput').value = bytesToMegabytes(selected.getAttribute('data-maxfilesize'));
            if (maxSizeInput) {
                maxSizeInput.value = selected.getAttribute('data-maxsize') || '200';
                normalizeMaxSizeInput();
            }
            document.getElementById('allTagsMustMatch').checked = selected.getAttribute('data-alltagsmustmatch') === "True" || selected.getAttribute('data-alltagsmustmatch') === "true";
            document.getElementById('createdAfterInput').value = selected.getAttribute('data-createdafter') || '';
            document.getElementById('createdBeforeInput').value = selected.getAttribute('data-createdbefore') || '';
            updateDynamicMatchCount();
        });
    }

    if (albumsInput) {
        albumsInput.addEventListener('change', updateDynamicMatchCount);
        if (window.jQuery) {
            jQuery(albumsInput).on('change select2:select select2:unselect', updateDynamicMatchCount);
        }
    }

    if (tagsInput) {
        tagsInput.addEventListener('change', updateDynamicMatchCount);
        if (window.jQuery) {
            jQuery(tagsInput).on('change select2:select select2:unselect', updateDynamicMatchCount);
        }
    }

    if (maxSizeInput) {
        maxSizeInput.addEventListener('input', normalizeMaxSizeInput);
        maxSizeInput.addEventListener('change', normalizeMaxSizeInput);
        normalizeMaxSizeInput();
    }

    if (relativeDateRangeButtons.length > 0) {
        relativeDateRangeButtons.forEach(function (button) {
            button.addEventListener('click', function () {
                const days = parseInt(button.getAttribute('data-relative-date-range'), 10);

                if (Number.isNaN(days) || days <= 0) return;

                applyRelativeDateRange(days);
            });
        });
    }

    updateDynamicMatchCount();

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
                MaxSize: maxSizeInput && maxSizeInput.value ? parseInt(maxSizeInput.value, 10) : null,
                MinFileSize: document.getElementById('minFileSizeInput').value ? megabytesToBytes(document.getElementById('minFileSizeInput').value) : null,
                MaxFileSize: document.getElementById('maxFileSizeInput').value ? megabytesToBytes(document.getElementById('maxFileSizeInput').value) : null,
                CreatedAfter: document.getElementById('createdAfterInput').value || null,
                CreatedBefore: document.getElementById('createdBeforeInput').value || null
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