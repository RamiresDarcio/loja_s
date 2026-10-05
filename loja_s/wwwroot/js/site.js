// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(() => {
    const carousel = document.querySelector("[data-carousel]");

    if (!carousel) {
        return;
    }

    const slides = Array.from(carousel.querySelectorAll(".carousel-slide"));
    const indicators = Array.from(carousel.querySelectorAll("[data-slide-to]"));
    const previousButton = carousel.querySelector(".carousel-previous");
    const nextButton = carousel.querySelector(".carousel-next");
    const toggleButton = carousel.querySelector(".carousel-toggle");
    const reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");

    if (slides.length < 2 || !previousButton || !nextButton || !toggleButton) {
        return;
    }

    let activeIndex = Math.max(0, slides.findIndex(slide => slide.classList.contains("is-active")));
    let timerId;
    let userPaused = reducedMotion.matches;
    let pointerInside = false;
    let focusInside = false;

    const showSlide = index => {
        activeIndex = (index + slides.length) % slides.length;

        slides.forEach((slide, slideIndex) => {
            const isActive = slideIndex === activeIndex;
            slide.classList.toggle("is-active", isActive);
            slide.setAttribute("aria-hidden", String(!isActive));
            slide.inert = !isActive;
        });

        indicators.forEach((indicator, indicatorIndex) => {
            const isActive = indicatorIndex === activeIndex;
            indicator.classList.toggle("is-active", isActive);
            indicator.setAttribute("aria-current", String(isActive));
        });
    };

    const stopTimer = () => {
        window.clearInterval(timerId);
        timerId = undefined;
    };

    const startTimer = () => {
        stopTimer();

        if (userPaused || pointerInside || focusInside || document.hidden || reducedMotion.matches) {
            return;
        }

        timerId = window.setInterval(() => showSlide(activeIndex + 1), 6000);
    };

    const updateToggle = () => {
        const isPaused = userPaused;
        toggleButton.disabled = reducedMotion.matches;
        toggleButton.textContent = isPaused ? "▶" : "Ⅱ";
        toggleButton.setAttribute(
            "aria-label",
            reducedMotion.matches
                ? "Troca automática desativada pela preferência de movimento reduzido"
                : isPaused
                    ? "Reproduzir troca automática"
                    : "Pausar troca automática"
        );
        toggleButton.setAttribute("aria-pressed", String(isPaused));
    };

    previousButton.addEventListener("click", () => {
        showSlide(activeIndex - 1);
        startTimer();
    });

    nextButton.addEventListener("click", () => {
        showSlide(activeIndex + 1);
        startTimer();
    });

    indicators.forEach(indicator => {
        indicator.addEventListener("click", () => {
            showSlide(Number(indicator.dataset.slideTo));
            startTimer();
        });
    });

    toggleButton.addEventListener("click", () => {
        userPaused = !userPaused;
        updateToggle();
        startTimer();
    });

    carousel.addEventListener("pointerenter", () => {
        pointerInside = true;
        stopTimer();
    });

    carousel.addEventListener("pointerleave", () => {
        pointerInside = false;
        startTimer();
    });

    carousel.addEventListener("focusin", () => {
        focusInside = true;
        stopTimer();
    });

    carousel.addEventListener("focusout", event => {
        if (!carousel.contains(event.relatedTarget)) {
            focusInside = false;
            startTimer();
        }
    });

    document.addEventListener("visibilitychange", startTimer);

    reducedMotion.addEventListener("change", () => {
        if (reducedMotion.matches) {
            userPaused = true;
        }

        updateToggle();
        startTimer();
    });

    showSlide(activeIndex);
    updateToggle();
    startTimer();
})();
