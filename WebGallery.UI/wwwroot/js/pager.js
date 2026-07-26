export function getPagerState(totalItems, pageSize, currentOffset, windowSize) {
    const safePageSize = Math.max(1, pageSize ?? 1);
    const safeWindowSize = Math.max(1, windowSize ?? 1);
    const totalPages = Math.max(1, Math.ceil((totalItems ?? 0) / safePageSize));
    const currentPage = Math.min(totalPages, Math.floor((currentOffset ?? 0) / safePageSize) + 1);

    const halfWindow = Math.floor(safeWindowSize / 2);
    let firstPage = Math.max(1, currentPage - halfWindow);
    let lastPage = Math.min(totalPages, firstPage + safeWindowSize - 1);
    firstPage = Math.max(1, lastPage - safeWindowSize + 1);

    return {
        totalPages,
        currentPage,
        firstPage,
        lastPage,
        previousPage: Math.max(1, currentPage - 1),
        nextPage: Math.min(totalPages, currentPage + 1),
        showPager: totalPages > 1,
    };
}

export function renderPager({
    ariaLabel,
    totalItems,
    pageSize,
    currentOffset,
    windowSize,
    buildPageUrl,
}) {
    const state = getPagerState(totalItems, pageSize, currentOffset, windowSize);
    if (!state.showPager) {
        return null;
    }

    const row = document.createElement("div");
    row.classList.add("row", "justify-content-center", "mt-4", "mb-3", "pager-row");

    const col = document.createElement("div");
    col.classList.add("col-auto", "pager-col");

    const nav = document.createElement("nav");
    nav.classList.add("pager-nav");
    nav.setAttribute("aria-label", ariaLabel ?? "Pages");

    const list = document.createElement("ul");
    list.classList.add("pagination", "mb-0", "pager-list");

    list.appendChild(createPageItem("First", buildPageUrl(1), state.currentPage === 1));
    list.appendChild(createPageItem("Previous", buildPageUrl(state.previousPage), state.currentPage === 1));

    for (let page = state.firstPage; page <= state.lastPage; page++) {
        list.appendChild(createPageItem(page.toString(), buildPageUrl(page), false, page === state.currentPage));
    }

    list.appendChild(createPageItem("Next", buildPageUrl(state.nextPage), state.currentPage === state.totalPages));
    list.appendChild(createPageItem("Last", buildPageUrl(state.totalPages), state.currentPage === state.totalPages));

    nav.appendChild(list);
    col.appendChild(nav);
    row.appendChild(col);
    return row;
}

function createPageItem(label, href, disabled, active = false) {
    const li = document.createElement("li");
    li.classList.add("page-item");

    if (disabled) {
        li.classList.add("disabled");
    }

    if (active) {
        li.classList.add("active");
    }

    const link = document.createElement("a");
    link.classList.add("page-link");
    link.textContent = label;

    if (disabled || active) {
        link.href = "#";
        link.setAttribute("tabindex", "-1");
        link.setAttribute("aria-disabled", "true");
    } else {
        link.href = href;
    }

    li.appendChild(link);
    return li;
}