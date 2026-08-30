---
description: "Use when creating or editing Razor views (.cshtml) or CSS that affects page layout/width, especially under Views/Admin. Ensures new pages account for the site-wide fixed sidebar defined in Views/Shared/_Layout.cshtml."
applyTo: "WebGallery.UI/Views/**/*.cshtml,WebGallery.UI/wwwroot/css/**/*.css"
---

# Site-Wide Sidebar Layout Rules

`Views/Shared/_Layout.cshtml` renders a `<header class="header-bar">` that is styled in
`wwwroot/css/shutter/style.css` as `position: fixed; right: 0; width: var(--sidebar-width)` (250px).
A fixed-position element does **not** reserve space in normal flow — it overlaps whatever
content is rendered underneath it. Pages must actively reserve that space themselves.

## Required pattern for every top-level view

Wrap all page body content in `<main class="main-content">`, and use `container-fluid`
(not `container`) for the inner content wrapper:

```cshtml
<main class="main-content">
<div class="container-fluid mt-5 ...">
    ...page content...
</div>
</main>
```

- `.main-content` applies `width: calc(100% - var(--sidebar-width))`, which is what actually
  prevents content from rendering under the fixed sidebar.
- Bootstrap's plain `.container` class caps width at fixed breakpoints computed from the
  *viewport* width, not the parent's available width — using it inside `.main-content` leaves
  a dead gap between content and the sidebar instead of filling the available space. Always use
  `.container-fluid` for pages nested in `.main-content`.
- Reference implementations: `Views/Albums/Index.cshtml`, `Views/Albums/Album.cshtml`,
  `Views/Bio/Index.cshtml`, `Views/Tags/Index.cshtml`, `Views/Single/Index.cshtml`.
- Do **not** add any extra sidebar-width compensation (e.g. `width: calc(100% - var(--sidebar-width))`
  or `margin-right: var(--sidebar-width)`) on elements nested inside `.main-content`. `.main-content`
  is the single source of truth for that compensation; stacking a second one on a child element
  (e.g. `.admin-page-shell`) double-subtracts the sidebar width and creates a large dead gap
  between content and the sidebar. Inner wrappers should just use `width: 100%`.

## Before shipping any new/edited admin or content page

1. Confirm the view's outer markup is wrapped in `<main class="main-content">`.
2. Confirm the inner container uses `container-fluid`, not `container`.
3. If a diagnostic screenshot shows content overlapping or hidden behind the right-hand nav
   (HOME / ALBUMS / SINGLE / BIO / TAGS / SEARCHES / ADMIN / LOGOUT), the missing wrapper is
   the first thing to check — not table/column CSS.

## Tables and action buttons

- Prefer normal browser auto table layout (no `table-layout: fixed` plus per-column
  `nth-child` percentage widths). Fixed layout + percentages compound unpredictably once
  nested inside `.main-content`/`.container-fluid` and can squeeze columns far narrower
  than expected.
- For an actions column with multiple inline buttons (Delete/Rename/Move, etc.), use
  `white-space: nowrap` on the `<td>` with small `margin-right` between buttons/forms.
  Do not use `display:flex; flex-wrap:wrap` for this — it causes buttons to stack
  vertically as soon as the column is even slightly tight.
