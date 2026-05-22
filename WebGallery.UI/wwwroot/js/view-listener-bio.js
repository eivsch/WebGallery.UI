function tagExists(tag) {
    var exists = false;
    var existingTags = $('#bioPictureTags').children();
    existingTags.each(function (index) {
        console.log(index + ": " + $(this).text());
        if (tag == $(this).text()) {
            exists = true;
            return false;
        }
    });

    return exists;
}

function addTag(tag) {
    $('#myInput').val('');
    var exists = tagExists(tag);
    var picId = $('#image-id-placeholder').text();
    var album = $('#album-id-placeholder').text();

    // Add
    if (!exists) {
        $.post(
            // Url
            "/bio/tag",
            // Data
            {
                tag: tag,
                pictureId: picId,
                album: album,
            }
            // OnSuccess
            , function () {
                $('#bioPictureTags').append('<li class="tag-list-item">' + tag + '</li>');
            });

        return true;
    }

    return false;
}

function deleteTag(tag, picId, album) {
    tag = tag.replace("#", "");
    $.post(
        // Url
        "/bio/tag/delete",
        // Data
        {
            tag: tag,
            pictureId: picId,
            album: album,
        }
        // OnSuccess
        , function () {
            var el = $("[data-tag-name='" + tag + "']");
            el.addClass("tag-list__strikethrough");
            setTimeout(() => { el.remove() }, 2000);
    });
}

$('#myInput').keyup(function (e) {
    var key = e.which;
    if (key == 13)  // the enter key code
    {
        var tag = $('#myInput').val();
        if (tag != '') {
            if (addTag(tag)) {
                $('#myDropdown').append('<a class="dropdown-item" onclick="addTag(\'' + tag + '\')" >' + tag + '</a>');
            }
        }
    }
    else
    {
        var input, filter, ul, li, a, i;
        input = document.getElementById("myInput");
        filter = input.value.toUpperCase();

        if (filter == "") {
            hideAll();
        }
        else {
            div = document.getElementById("myDropdown");
            a = div.getElementsByTagName("a");

            var counter = 0;
            for (i = 0; i < a.length; i++) {
                if (counter == 10) {
                    break;
                }

                txtValue = a[i].textContent || a[i].innerText;
                if (txtValue.toUpperCase().indexOf(filter) > -1) {
                    a[i].style.display = "";
                    counter++;
                } else {
                    a[i].style.display = "none";
                }
            }
        }
    }
});

function hideAll() {
    div = document.getElementById("myDropdown");
    a = div.getElementsByTagName("a");
    for (i = 0; i < a.length; i++) {
        a[i].style.display = "none";
    }
}

function focusInput() {
    const input = document.getElementById("myInput");
    const dropdown = document.getElementById("myDropdown");

    // Toggle visibility of the input and dropdown
    if (input.style.display === "none") {
        input.style.display = "block";
        dropdown.style.display = "block";
        input.focus(); // Focus on the input field
    } else {
        input.style.display = "none";
        dropdown.style.display = "none";
    }
}

function viewGeneratedImages(button) {
    const container = document.getElementById('generatedVideoImagesContainer');
    if (!button || !container) {
        return;
    }

    if (container.dataset.loaded === 'true') {
        const isVisible = container.style.display !== 'none';
        container.style.display = isVisible ? 'none' : 'block';
        return;
    }

    const album = button.getAttribute('data-album') || '';
    const fileNameNoExt = button.getAttribute('data-file-name-no-ext') || '';
    if (album === '' || fileNameNoExt === '') {
        return;
    }

    const originalText = button.textContent;
    button.disabled = true;
    button.textContent = 'Loading...';

    fetch('/single/search?albums=' + encodeURIComponent(album) + '&mediaNameContains=' + encodeURIComponent(fileNameNoExt), {
        method: 'GET',
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
    })
    .then(function (response) {
        if (!response.ok) {
            throw new Error('Failed to load generated images');
        }

        return response.text();
    })
    .then(function (html) {
        const parser = new DOMParser();
        const doc = parser.parseFromString(html, 'text/html');
        const photosContainer = doc.querySelector('main.main-content .container-fluid.photos');

        container.innerHTML = '';

        if (photosContainer) {
            const galleryLink = document.createElement('a');
            const searchUrl = '/single/search?albums=' + encodeURIComponent(album) + '&mediaNameContains=' + encodeURIComponent(fileNameNoExt);
            galleryLink.href = searchUrl;
            galleryLink.textContent = 'View as gallery';
            container.appendChild(galleryLink);

            const items = document.createElement('ul');
            items.className = 'bio-generated-images__list';

            const mediaLinks = photosContainer.querySelectorAll('a.photo-item');
            mediaLinks.forEach(function (link) {
                const fileName = link.getAttribute('data-name') || '';
                if (!fileName) {
                    return;
                }

                const listItem = document.createElement('li');
                listItem.className = 'bio-generated-images__item';
                listItem.setAttribute('data-name', fileName);
                listItem.setAttribute('role', 'button');
                listItem.setAttribute('tabindex', '0');
                listItem.textContent = fileName;
                items.appendChild(listItem);
            });

            if (items.children.length > 0) {
                container.appendChild(items);
            } else {
                container.innerHTML = '<p>No associated image names were found.</p>';
            }
        }

        container.dataset.loaded = 'true';
        container.style.display = 'block';
    })
    .catch(function () {
        container.innerHTML = '<p>Unable to load associated images.</p>';
        container.dataset.loaded = 'true';
        container.style.display = 'block';
    })
    .finally(function () {
        button.disabled = false;
        button.textContent = originalText;
    });
}

function bioSwitch(album, sortOrder) {
    window.history.pushState("", "", '/Bio/' + album + '/' + sortOrder);
    setupVideoThumbnailButton();
}

function setupVideoThumbnailButton() {
    var video = document.querySelector('video');
    var setThumbBtn = document.getElementById('setVideoThumbnail');
    var genImgBtn = document.getElementById('generateVideoImage');
    function formatTime(seconds) {
        var totalMs = Math.ceil(seconds * 1000);
        var h = Math.floor(totalMs / 3600000).toString().padStart(2, '0');
        var m = Math.floor((totalMs % 3600000) / 60000).toString().padStart(2, '0');
        var s = Math.floor((totalMs % 60000) / 1000).toString().padStart(2, '0');
        var ms = (totalMs % 1000).toString().padStart(3, '0');
        return h + ":" + m + ":" + s + "." + ms;
    }
    if (video && setThumbBtn) {
        setThumbBtn.onclick = function () {
            var currentTime = formatTime(video.currentTime);
            var appPathB64 = setThumbBtn.getAttribute('data-app-path-b64') || window.appPathBase64 || "";
            setThumbBtn.disabled = true;
            setThumbBtn.textContent = "Setting...";
            fetch('/Bio/SetVideoThumbnail', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ appPathB64: appPathB64, currentTime: currentTime })
            })
            .then(function(response) {
                if (response.ok) {
                    setThumbBtn.textContent = "Thumbnail set!";
                    setTimeout(function () {
                        setThumbBtn.textContent = "Set as thumbnail";
                        setThumbBtn.disabled = false;
                    }, 1500);
                } else {
                    setThumbBtn.textContent = "Failed!";
                    setThumbBtn.disabled = false;
                }
            })
            .catch(function () {
                setThumbBtn.textContent = "Failed!";
                setThumbBtn.disabled = false;
            });
        };
    }
    if (video && genImgBtn) {
        genImgBtn.onclick = function () {
            var currentTime = formatTime(video.currentTime);
            var appPathB64 = genImgBtn.getAttribute('data-app-path-b64') || window.appPathBase64 || "";
            genImgBtn.disabled = true;
            genImgBtn.textContent = "Generating...";
            fetch('/Bio/GenerateVideoImage', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ appPathB64: appPathB64, currentTime: currentTime })
            })
            .then(function(response) {
                if (response.ok) {
                    genImgBtn.textContent = "Image generated!";
                    setTimeout(function () {
                        genImgBtn.textContent = "Generate image";
                        genImgBtn.disabled = false;
                    }, 1500);
                } else {
                    genImgBtn.textContent = "Failed!";
                    genImgBtn.disabled = false;
                }
            })
            .catch(function () {
                genImgBtn.textContent = "Failed!";
                genImgBtn.disabled = false;
            });
        };
    }

    var generatedImagesBtn = document.getElementById('viewGeneratedImages');
    if (generatedImagesBtn) {
        generatedImagesBtn.onclick = function () {
            viewGeneratedImages(generatedImagesBtn);
        };
    }

    var generatedImagesContainer = document.getElementById('generatedVideoImagesContainer');
    if (generatedImagesContainer) {
        generatedImagesContainer.onclick = function (event) {
            var target = event.target.closest('.bio-generated-images__item');
            if (!target) {
                return;
            }

            var fileName = target.getAttribute('data-name') || target.textContent || '';
            seekVideoToGeneratedImageTimestamp(fileName);
        };

        generatedImagesContainer.onkeydown = function (event) {
            if (event.key !== 'Enter' && event.key !== ' ') {
                return;
            }

            var target = event.target.closest('.bio-generated-images__item');
            if (!target) {
                return;
            }

            event.preventDefault();
            var fileName = target.getAttribute('data-name') || target.textContent || '';
            seekVideoToGeneratedImageTimestamp(fileName);
        };
    }
}

function seekVideoToGeneratedImageTimestamp(fileName) {
    var video = document.querySelector('video');
    if (!video || !fileName) {
        return;
    }

    var seconds = parseGeneratedImageTimestamp(fileName);
    if (seconds === null) {
        return;
    }

    var setCurrentTime = function () {
        try {
            if (video.tabIndex < 0) {
                video.tabIndex = -1;
            }
            
            video.currentTime = seconds;
            video.focus();
        }
        catch (error) {
            console.error('Unable to seek video to generated image timestamp.', error);
        }
    };

    if (video.readyState >= 1) {
        setCurrentTime();
        return;
    }

    video.addEventListener('loadedmetadata', setCurrentTime, { once: true });
}

function parseGeneratedImageTimestamp(fileName) {
    var fileNameNoExt = fileName.replace(/\.jpg$/i, '');
    var underscoreIndex = fileNameNoExt.lastIndexOf('_');
    if (underscoreIndex < 0) {
        return null;
    }

    var timestamp = fileNameNoExt.substring(underscoreIndex + 1);
    var timestampParts = timestamp.split('.');
    if (timestampParts.length !== 2 || timestampParts[0].length !== 6 || timestampParts[1].length !== 3) {
        return null;
    }

    var hours = parseInt(timestampParts[0].substring(0, 2), 10);
    var minutes = parseInt(timestampParts[0].substring(2, 4), 10);
    var seconds = parseInt(timestampParts[0].substring(4, 6), 10);
    var milliseconds = parseInt(timestampParts[1], 10);

    var totalSeconds = (hours * 3600) + (minutes * 60) + seconds + (milliseconds / 1000);
    
    return totalSeconds;
}

// Initialize the video thumbnail button on page load
document.addEventListener('DOMContentLoaded', function() {
    setupVideoThumbnailButton();
});