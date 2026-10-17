function toggleTheme() {
    const theme = document.documentElement.dataset.bsTheme === "dark" ? "light" : "dark";
    document.documentElement.dataset.bsTheme = theme;
    document.cookie = `pisco-theme=${theme};path=/;max-age=31536000`;
}
