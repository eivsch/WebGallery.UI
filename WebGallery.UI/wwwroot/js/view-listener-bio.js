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

function bioSwitch(album, sortOrder) {
    window.history.pushState("", "", '/Bio/' + album + '/' + sortOrder);
}