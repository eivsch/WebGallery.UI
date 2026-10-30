const headline = document.querySelector('h2.text-white');
headline.textContent = "Custom site";


const queryString = window.location.search.substring(1);
console.log(queryString);
const queryParams = parse_query_string(queryString);

const tagsContainer = document.querySelector('div.row.align-items-stretch');
tagsContainer.textContent = "lol?????" + queryParams.mediaCount;



function parse_query_string(query) {
    var vars = query.split("&");
    var query_string = {};
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        var key = decodeURIComponent(pair.shift());
        var value = decodeURIComponent(pair.join("="));
        // If first entry with this name
        if (typeof query_string[key] === "undefined") {
            query_string[key] = value;
            // If second entry with this name
        } else if (typeof query_string[key] === "string") {
            var arr = [query_string[key], value];
            query_string[key] = arr;
            // If third or later entry with this name
        } else {
            query_string[key].push(value);
        }
    }
    return query_string;
}