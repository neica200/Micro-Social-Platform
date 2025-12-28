(() => {
    const input = document.getElementById("liveSearchInput");
    const results = document.getElementById("liveSearchResults");
    if (!input || !results) return;

    let timer = null;
    const MIN_LEN = 2;
    const DEBOUNCE_MS = 250;

    const hide = () => {
        results.style.display = "none";
        results.innerHTML = "";
    };

    const show = () => {
        results.style.display = "block";
    };

    const escapeHtml = (s) =>
        String(s)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");

    // prevent refresh on Enter while typing (submit ramane doar pe click pe buton)
    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter") e.preventDefault();
        if (e.key === "Escape") hide();
    });

    input.addEventListener("input", () => {
        const q = input.value.trim();
        clearTimeout(timer);

        timer = setTimeout(async () => {
            if (q.length < MIN_LEN) {
                hide();
                return;
            }

            results.innerHTML = `<div class="list-group-item">Searching...</div>`;
            show();

            try {
                const resp = await fetch(`/Search/Live?q=${encodeURIComponent(q)}`);
                if (!resp.ok) throw new Error("Network error");
                const data = await resp.json();

                if (!data || data.length === 0) {
                    results.innerHTML = `<div class="list-group-item">No results</div>`;
                    return;
                }

                results.innerHTML = data.map(p => {
                    const name = escapeHtml(p.fullName || p.userName || "User");
                    const username = p.userName ? ` 📧 ${escapeHtml(p.userName)}` : "";
                    const lock = p.isPrivate ? ` <span class="text-muted">🔒</span>` : "";
                    const avatar = p.avatar
                        ? `<img src="${escapeHtml(p.avatar)}" alt="" style="width:28px;height:28px;border-radius:50%;object-fit:cover;margin-right:10px;">`
                        : "";

                    // ✅ profilul tau:
                    const url = `/Profiles/Index?id=${encodeURIComponent(p.userId)}`;

                    return `
            <a class="list-group-item list-group-item-action d-flex align-items-center"
               href="${url}">
              ${avatar}
              <div class="d-flex flex-column">
                <strong>${name}${lock}</strong>
                <small class="text-muted">${username}</small>
              </div>
            </a>
          `;
                }).join("");
            } catch (e) {
                hide();
            }
        }, DEBOUNCE_MS);
    });

    // click outside => close
    document.addEventListener("click", (e) => {
        if (e.target !== input && !results.contains(e.target)) hide();
    });
})();
    