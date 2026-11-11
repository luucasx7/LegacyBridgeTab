const status = document.getElementById("status");
document.getElementById("open").addEventListener("click", async () => {
  status.textContent = "Connecting to Legacy Bridge…";
  try {
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
    if (!tab || !tab.url) throw new Error("Could not read the address of this tab.");
    const url = new URL(tab.url);
    if (!["http:", "https:"].includes(url.protocol)) {
      throw new Error("Open an HTTP or HTTPS page first.");
    }
    chrome.runtime.sendNativeMessage(
      "com.codex.legacybridge",
      { action: "open", url: tab.url },
      (response) => {
        if (chrome.runtime.lastError) {
          status.textContent = "Legacy Bridge is not installed. See the README for setup instructions.";
          return;
        }
        status.textContent = response && response.ok
          ? "Page opened in Legacy Bridge."
          : (response && response.error) || "Could not open the page.";
      }
    );
  } catch (error) {
    status.textContent = error.message;
  }
});
