const status = document.getElementById("status");
document.getElementById("open").addEventListener("click", async () => {
  status.textContent = "Conectando ao programa auxiliar…";
  try {
    const [tab] = await chrome.tabs.query({ active: true, currentWindow: true });
    if (!tab || !tab.url) throw new Error("Não consegui ler o endereço desta aba.");
    const url = new URL(tab.url);
    if (!["http:", "https:"].includes(url.protocol)) {
      throw new Error("Abra primeiro uma página http ou https.");
    }
    chrome.runtime.sendNativeMessage(
      "com.codex.legacybridge",
      { action: "open", url: tab.url },
      (response) => {
        if (chrome.runtime.lastError) {
          status.textContent = "Auxiliar não instalado. Consulte o README.";
          return;
        }
        status.textContent = response && response.ok
          ? "Página aberta na janela auxiliar."
          : (response && response.error) || "Não foi possível abrir a página.";
      }
    );
  } catch (error) {
    status.textContent = error.message;
  }
});
