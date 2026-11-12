function toggleTheme() {
    const theme = document.documentElement.dataset.bsTheme === "dark" ? "light" : "dark";
    document.documentElement.dataset.bsTheme = theme;
    document.cookie = `pisco-theme=${theme};path=/;max-age=31536000`;
}

function piscoCloseDrawer() {
    if (window.matchMedia("(max-width: 991.98px)").matches)
        document.getElementById("sidebar-drawer").checked = false;
}
