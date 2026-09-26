// Placeholder reviews: fictional people and organizations.
const REVIEWS = [
  {
    quote:
      "We Onboard Members Across British Columbia In Minutes Now. TrustCheck Gives Every Check A Clear Status, Result And Reason.",
    name: "Amelia Tremblay",
    role: "Compliance Lead, Maple Ridge Credit Union",
  },
  {
    quote: "Our Fraud Team Stopped Chasing Documents. Every Rejected Check Tells Us Exactly Why, Right In The Record.",
    name: "Daniel Okafor",
    role: "Risk Manager, North Shore Fintech",
  },
  {
    quote: "The API Was Live In An Afternoon. Statuses Move From Submitted To Completed Exactly When They Should.",
    name: "Priya Raman",
    role: "Engineering Lead, Lakeview Payments",
  },
  {
    quote: "One Screen For Every Verification We Run Across BC. Audits Went From Weeks Of Digging To A Single Export.",
    name: "Marc-Antoine Gagnon",
    role: "Operations Director, Okanagan Capital",
  },
  {
    quote: "Clear, Fast And Easy To Trust. New Customers Get Verified Before Their Coffee Gets Cold.",
    name: "Hannah Whitford",
    role: "Head of Onboarding, Fraser Valley Lending",
  },
];

const prefersReducedMotion = matchMedia("(prefers-reduced-motion: reduce)").matches;

initReviewCarousel();
initSignupForm();

function initReviewCarousel() {
  const quote = document.getElementById("review-quote");
  const name = document.getElementById("review-name");
  const role = document.getElementById("review-role");
  const index = document.getElementById("review-index");
  const initials = document.getElementById("review-initials");
  let current = 0;

  const show = (next) => {
    current = (next + REVIEWS.length) % REVIEWS.length;
    const review = REVIEWS[current];
    renderQuote(quote, review.quote);
    name.textContent = review.name;
    role.textContent = review.role;
    initials.textContent = toInitials(review.name);
    index.textContent = String(current + 1).padStart(2, "0");
  };

  document.querySelector(".review-prev").addEventListener("click", () => show(current - 1));
  document.querySelector(".review-next").addEventListener("click", () => show(current + 1));
}

function renderQuote(element, text) {
  if (prefersReducedMotion) {
    element.textContent = text;
    return;
  }
  const words = text.split(" ").flatMap((word, i) => {
    const span = document.createElement("span");
    span.className = "word";
    span.style.setProperty("--i", i);
    span.textContent = word;
    return [span, " "];
  });
  element.replaceChildren(...words);
}

function toInitials(fullName) {
  return fullName
    .split(" ")
    .map((part) => part[0])
    .join("")
    .slice(0, 2);
}

// Static demo page: there is no signup backend yet, so keep the visitor on the page.
function initSignupForm() {
  document.querySelector(".signup-form").addEventListener("submit", (event) => event.preventDefault());
}
