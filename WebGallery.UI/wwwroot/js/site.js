// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


/**
 *  SECTION: Tags (Bio)
 */

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

function formatTag(tag) {
    if (!tag.startsWith('#')) {
        tag = '#' + tag;
    }

    return tag;
}

function addTag(tag) {
    // Cleanup field
    $('#myInput').val('');
    // Format the string
    tag = formatTag(tag);
    // Check that it doesn't already exist
    var exists = tagExists(tag);
    // Add
    if (!exists) {
        $.post(
            // Url
            "/bio",
            // Data
            {
                tag: tag
            }
            // OnSuccess
            , function () {
                $('#bioPictureTags').append('<li class="tag-list-item">' + tag + '</li>');
            });

        return true;
    }

    return false;
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
    $('#myInput').toggle();
    $('#myInput').focus();
}

/**
 *  END SECTION: Tags (Bio)
 */