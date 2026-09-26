const prefersReducedMotion = matchMedia("(prefers-reduced-motion: reduce)").matches;
const hasFinePointer = matchMedia("(pointer: fine)").matches;

initStickyHeader();
initNavIndicator();

if (!prefersReducedMotion) {
  // Motion CSS only applies under .motion, so the page is fully visible without JS.
  document.documentElement.classList.add("motion");
  initReveal();
  initCounters();
  initOdometers();
  initParallax();
  if (hasFinePointer) {
    initCardLight();
    initMagneticButtons();
  }
}

function onScrollFrame(callback) {
  let isPending = false;
  const schedule = () => {
    if (isPending) return;
    isPending = true;
    requestAnimationFrame(() => {
      isPending = false;
      callback();
    });
  };
  addEventListener("scroll", schedule, { passive: true });
  return schedule;
}

function initStickyHeader() {
  const STUCK_AFTER_PX = 12;
  const header = document.querySelector(".site-header");
  const update = () => header.classList.toggle("is-stuck", scrollY > STUCK_AFTER_PX);
  onScrollFrame(update);
  update();
}

function initNavIndicator() {
  const CLICK_LOCK_MS = 1200;
  const ACTIVE_LINE_RATIO = 0.35;
  const list = document.querySelector(".nav-pill");
  const links = [...list.querySelectorAll("a")];
  const sections = links.map((link) => document.querySelector(link.getAttribute("href")));
  const indicator = createNavIndicator(list);
  let active;
  let lockedUntil = 0;

  const setActive = (link) => {
    if (link === active) return;
    active?.classList.remove("is-active");
    active?.removeAttribute("aria-current");
    link.classList.add("is-active");
    link.setAttribute("aria-current", "location");
    active = link;
    indicator.moveTo(link);
  };

  const followScroll = () => {
    if (performance.now() < lockedUntil) return;
    const line = innerHeight * ACTIVE_LINE_RATIO;
    let current = links[0];
    let closestTop = -Infinity;
    sections.forEach((section, i) => {
      const top = section.getBoundingClientRect().top;
      if (top <= line && top > closestTop) {
        closestTop = top;
        current = links[i];
      }
    });
    setActive(current);
  };

  // While a nav click is scrolling, hold the pill on the clicked link instead of chasing passed sections.
  for (const link of links) {
    link.addEventListener("click", () => {
      lockedUntil = performance.now() + CLICK_LOCK_MS;
      setActive(link);
    });
  }
  const schedule = onScrollFrame(followScroll);
  addEventListener("scrollend", () => {
    lockedUntil = 0;
    schedule();
  });
  addEventListener("resize", () => active && indicator.moveTo(active));

  for (const link of links) link.classList.remove("is-active");
  indicator.withoutTransition(followScroll);
  document.fonts.ready.then(() => indicator.withoutTransition(() => indicator.moveTo(active)));
}

function createNavIndicator(list) {
  // Lives on the <nav>, not the <ul>: a list may only contain <li> items.
  const host = list.parentElement;
  const pill = document.createElement("span");
  pill.className = "nav-indicator";
  pill.setAttribute("aria-hidden", "true");
  host.prepend(pill);
  list.classList.add("has-indicator");

  return {
    moveTo(link) {
      const hostBox = host.getBoundingClientRect();
      const box = link.getBoundingClientRect();
      pill.style.width = `${box.width}px`;
      pill.style.height = `${box.height}px`;
      pill.style.transform = `translate(${box.left - hostBox.left}px, ${box.top - hostBox.top}px)`;
    },
    withoutTransition(update) {
      pill.style.transition = "none";
      update();
      requestAnimationFrame(() => {
        pill.style.transition = "";
      });
    },
  };
}

function initReveal() {
  const REVEAL_THRESHOLD = 0.18;
  staggerDelay(".dash [data-reveal]", "--d", 0.75, 0.12);
  staggerDelay(".stats [data-reveal]", "--d", 0, 0.12);
  staggerDelay(".flag", "--fd", 1.5, 0.8);
  document.querySelectorAll(".pill-bars span, .stat-bars span, .segbar i").forEach((el) => {
    el.style.setProperty("--i", [...el.parentElement.children].indexOf(el));
  });

  const observer = new IntersectionObserver(
    (entries) => {
      for (const entry of entries) {
        if (!entry.isIntersecting) continue;
        entry.target.classList.add("is-visible");
        observer.unobserve(entry.target);
      }
    },
    { threshold: REVEAL_THRESHOLD },
  );
  for (const el of document.querySelectorAll("[data-reveal], .float-card, .site-footer")) observer.observe(el);
}

function staggerDelay(selector, property, startSeconds, stepSeconds) {
  document.querySelectorAll(selector).forEach((el, i) => {
    el.style.setProperty(property, `${startSeconds + i * stepSeconds}s`);
  });
}

function initCounters() {
  const COUNT_MS = 1600;
  const HERO_DELAY_MS = 950;
  const SECTION_DELAY_MS = 150;
  const counters = document.querySelectorAll("[data-count]");

  const format = (el, value) => {
    const decimals = Number(el.dataset.decimals || 0);
    return value.toLocaleString("en-CA", { minimumFractionDigits: decimals, maximumFractionDigits: decimals });
  };

  const run = (el) => {
    const target = Number(el.dataset.count);
    let start;
    const step = (now) => {
      start ??= now;
      const progress = Math.min((now - start) / COUNT_MS, 1);
      const easeOutQuart = 1 - (1 - progress) ** 4;
      el.textContent = format(el, target * easeOutQuart);
      if (progress < 1) requestAnimationFrame(step);
    };
    requestAnimationFrame(step);
    // rAF pauses in background tabs; make sure the real value always lands.
    setTimeout(() => {
      el.textContent = format(el, target);
    }, COUNT_MS + 100);
  };

  counters.forEach((el) => {
    el.textContent = format(el, 0);
  });
  const observer = new IntersectionObserver(
    (entries) => {
      for (const entry of entries) {
        if (!entry.isIntersecting) continue;
        observer.unobserve(entry.target);
        const delay = entry.target.closest(".dash") ? HERO_DELAY_MS : SECTION_DELAY_MS;
        setTimeout(() => run(entry.target), delay);
      }
    },
    { threshold: 0.6 },
  );
  for (const el of counters) observer.observe(el);
}

function initOdometers() {
  const START_MS = 350;
  const STAGGER_SECONDS = 0.09;
  const SPINS_BEFORE_LANDING = 2;
  const ROW_HEIGHT_EM = 1.2;

  document.querySelectorAll("[data-odometer]").forEach((el) => {
    const text = el.textContent.trim();
    el.setAttribute("aria-label", text);
    const columns = [];

    const parts = [...text].map((char, i) => {
      if (!/\d/.test(char)) {
        const separator = document.createElement("span");
        separator.textContent = char;
        return separator;
      }
      const strip = document.createElement("span");
      strip.className = "odo-strip";
      strip.style.setProperty("--d", `${i * STAGGER_SECONDS}s`);
      const lastRow = SPINS_BEFORE_LANDING * 10 + Number(char);
      for (let n = 0; n <= lastRow; n++) {
        const row = document.createElement("span");
        row.textContent = n % 10;
        strip.append(row);
      }
      const digit = document.createElement("span");
      digit.className = "odo-digit";
      digit.setAttribute("aria-hidden", "true");
      digit.append(strip);
      columns.push({ strip, lastRow });
      return digit;
    });

    el.replaceChildren(...parts);
    setTimeout(() => {
      for (const { strip, lastRow } of columns) {
        strip.style.transform = `translateY(-${lastRow * ROW_HEIGHT_EM}em)`;
      }
    }, START_MS);
    // Swap back to plain text once the last column lands, so the settled number renders crisply.
    columns.at(-1)?.strip.addEventListener(
      "transitionend",
      () => {
        el.textContent = text;
      },
      { once: true },
    );
  });
}

function initCardLight() {
  const hero = document.querySelector(".hero");
  const hand = document.querySelector(".hand");
  const face = document.querySelector(".card-face");
  let frame = 0;

  hero.addEventListener("pointermove", (event) => {
    cancelAnimationFrame(frame);
    frame = requestAnimationFrame(() => {
      const heroBox = hero.getBoundingClientRect();
      const faceBox = face.getBoundingClientRect();
      hand.style.setProperty("--px", ((event.clientX - heroBox.left) / heroBox.width - 0.5) * 2);
      hand.style.setProperty("--py", ((event.clientY - heroBox.top) / heroBox.height - 0.5) * 2);
      face.style.setProperty("--mx", (event.clientX - faceBox.left) / faceBox.width);
      face.style.setProperty("--my", (event.clientY - faceBox.top) / faceBox.height);
    });
  });
  hero.addEventListener("pointerleave", () => {
    hand.style.setProperty("--px", 0);
    hand.style.setProperty("--py", 0);
  });
}

function initMagneticButtons() {
  const PULL = 0.3;
  document.querySelectorAll(".btn, .review-nav button, .signup-form button").forEach((button) => {
    button.addEventListener("pointermove", (event) => {
      const box = button.getBoundingClientRect();
      const dx = event.clientX - (box.left + box.width / 2);
      const dy = event.clientY - (box.top + box.height / 2);
      button.style.transform = `translate(${dx * PULL}px, ${dy * PULL}px)`;
    });
    button.addEventListener("pointerleave", () => {
      button.style.transform = "";
    });
  });
}

function initParallax() {
  const WIDE_LAYOUT_PX = 1200;
  const section = document.querySelector(".features");
  const layers = [
    { el: document.querySelector(".card-results"), depth: -70 },
    { el: document.querySelector(".card-signals"), depth: 55 },
    { el: document.querySelector(".id-types"), depth: -35 },
    { el: document.querySelector(".rating"), depth: 25 },
  ];

  const update = () => {
    const isWide = innerWidth > WIDE_LAYOUT_PX;
    const box = section.getBoundingClientRect();
    const progress = (innerHeight - box.top) / (innerHeight + box.height) - 0.5;
    for (const { el, depth } of layers) {
      el.style.transform = isWide ? `translateY(${progress * depth * 2}px)` : "";
    }
  };
  onScrollFrame(update);
  addEventListener("resize", update);
  update();
}
