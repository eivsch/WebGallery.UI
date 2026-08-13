function tagExists(tag) {
    var normalised = tag.replace(/^#/, '');
    return $('#bioPictureTags').children('[data-tag-name="' + normalised + '"]').length > 0;
}

function addTag(tag) {
    $('#myInput').val('');
    var exists = tagExists(tag);
    var picId = $('#image-id-placeholder').text();
    var album = $('#album-id-placeholder').text();

    // Add
    if (!exists) {
        var normalised = tag.replace(/^#/, '');
        var displayTag = tag.startsWith('#') ? tag : '#' + tag;
        $.post(
            // Url
            "/bio/tag",
            // Data
            {
                tag: normalised,
                pictureId: picId,
                album: album,
            }
            // OnSuccess
            , function () {
                var li = $('<li class="tag-list-item"></li>')
                    .attr('data-tag-name', normalised)
                    .text(displayTag + ' ')
                    .append(
                        $('<i class="far fa-minus-square"></i>').on('click', function () {
                            deleteTag(normalised, picId, album);
                        })
                    );
                $('#bioPictureTags').append(li);
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
            const fileNames = [];
            mediaLinks.forEach(function (link) {
                const fileName = link.getAttribute('data-name') || '';
                if (!fileName) {
                    return;
                }

                fileNames.push(fileName);
            });

            fileNames.sort(function (a, b) {
                return a.localeCompare(b);
            });

            fileNames.forEach(function (fileName) {
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

    setupVideoEnlargeToggle();

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

function toggleVideoFullscreen(video) {
    if (document.fullscreenElement || document.webkitFullscreenElement) {
        if (document.exitFullscreen) {
            document.exitFullscreen();
        } else if (document.webkitExitFullscreen) {
            document.webkitExitFullscreen();
        }
    } else if (video.requestFullscreen) {
        video.requestFullscreen();
    } else if (video.webkitRequestFullscreen) {
        video.webkitRequestFullscreen();
    } else if (video.webkitEnterFullscreen) {
        video.webkitEnterFullscreen();
    }
}

function setEnlargedVideoView(enlarged) {
    document.body.classList.toggle('theater-mode', enlarged);
    var toggleBtn = document.getElementById('videoEnlargeToggle');
    if (toggleBtn) {
        toggleBtn.innerHTML = enlarged
            ? '<i class="fas fa-compress-alt"></i> Exit theater mode'
            : '<i class="fas fa-expand-alt"></i> Theater mode';
    }
}

function setupVideoEnlargeToggle() {
    var toggleBtn = document.getElementById('videoEnlargeToggle');
    if (!toggleBtn) {
        // Navigated to a non-video item; drop out of theater mode.
        document.body.classList.remove('theater-mode');
        return;
    }

    toggleBtn.onclick = function () {
        setEnlargedVideoView(!document.body.classList.contains('theater-mode'));
    };
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

function showBioLoading() {
    var el = document.getElementById('bio-loading');
    if (el) el.style.display = 'block';
}

function hideBioLoading() {
    var el = document.getElementById('bio-loading');
    if (el) el.style.display = 'none';
}

// iOS Safari can navigate an <a> before onclick="return confirm(...)" resolves, so
// intercept the click explicitly and only follow the link once confirmed.
document.addEventListener('click', function (e) {
    var deleteLink = e.target.closest('.bio-delete-link');
    if (!deleteLink) return;

    e.preventDefault();
    var message = deleteLink.getAttribute('data-confirm-message') || 'Are you sure you want to delete this item? This cannot be undone.';
    if (window.confirm(message)) {
        window.location.href = deleteLink.getAttribute('href');
    }
});

document.addEventListener('keydown', function(e) {
    if (e.target.tagName === 'INPUT' || e.target.tagName === 'TEXTAREA') return;
    if (e.defaultPrevented) return;

    if (e.key === 'Escape') {
        if (document.body.classList.contains('theater-mode')) {
            setEnlargedVideoView(false);
        }
        return;
    }

    var video = document.querySelector('video.bio-img');

    // Take over space/F ourselves so the browser's default handling (which can
    // exit fullscreen instead of just pausing) never kicks in.
    if (e.code === 'Space' || e.key === ' ') {
        if (video) {
            e.preventDefault();
            if (video.paused) {
                video.play();
            } else {
                video.pause();
            }
        }
        return;
    }

    if (e.key === 'f' || e.key === 'F') {
        if (video) {
            e.preventDefault();
            toggleVideoFullscreen(video);
        }
        return;
    }

    if (e.key === 't' || e.key === 'T') {
        var theaterToggle = document.getElementById('videoEnlargeToggle');
        if (theaterToggle) theaterToggle.click();
        return;
    }

    if (e.key === 'x' || e.key === 'X') {
        var generateImageBtn = document.getElementById('generateVideoImage');
        if (generateImageBtn) generateImageBtn.click();
        return;
    }

    // Native arrow-key seeking only works while the video element itself has focus (e.g. after
    // clicking a button it's lost), so seek manually here instead of relying on the browser default.
    if (video && (e.key === 'ArrowLeft' || e.key === 'ArrowRight')) {
        e.preventDefault();
        video.currentTime += e.key === 'ArrowLeft' ? -5 : 5;
        return;
    }

    // Prev/next keyboard navigation conflicts with video seek shortcuts, so skip it for videos.
    var mediaTypeEl = document.getElementById('media-type-placeholder');
    if (mediaTypeEl && mediaTypeEl.textContent.trim() === 'Video') return;

    if (e.key === 'ArrowLeft') {
        var prev = document.querySelector('.bio-nav-prev');
        if (prev) prev.click();
    } else if (e.key === 'ArrowRight') {
        var next = document.querySelector('.bio-nav-next');
        if (next) next.click();
    }
});