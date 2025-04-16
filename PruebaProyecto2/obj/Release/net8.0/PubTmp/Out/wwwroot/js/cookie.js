document.addEventListener("DOMContentLoaded", function () {
    const cookieBanner = document.getElementById("cookie-banner");
    const acceptButton = document.getElementById("cookie-accept");
    const cancelButton = document.getElementById("cookie-cancel");

    // Mostrar el banner si no se ha aceptado
    if (!localStorage.getItem("cookiesAccepted")) {
        cookieBanner.classList.remove("hide");
    }

    // Aceptar cookies: ocultar y guardar en localStorage
    acceptButton.addEventListener("click", function () {
        cookieBanner.classList.add("hide");
        localStorage.setItem("cookiesAccepted", "true");
    });

    // Cancelar cookies: solo ocultar visualmente
    cancelButton.addEventListener("click", function () {
        cookieBanner.classList.add("hide");
    });
});
