# Legacy Bridge

A Chrome extension and Windows helper for opening the current tab in a separate window that uses the Windows WebBrowser (MSHTML) engine. This can help with older websites that do not work correctly in modern browsers.

## What you need

- Windows 10 or 11 with .NET Framework and the Windows WebBrowser component.
- Google Chrome.
- Windows PowerShell.

## Install

1. On this GitHub page, click the green **Code** button, then choose **Download ZIP**.
2. In File Explorer, open the downloaded ZIP and select **Extract All**. Open the extracted `LegacyBridgeTab-main` folder.
3. In Chrome, go to `chrome://extensions`, turn on **Developer mode**, and click **Load unpacked**. Select the `extension` folder inside `LegacyBridgeTab-main`.
4. In File Explorer, open the extracted `LegacyBridgeTab-main` folder. Click the address bar, type `powershell`, and press **Enter**.
5. In the PowerShell window, run these commands one at a time:

   ```powershell
   .\build.ps1
   .\install.ps1
   ```

6. Restart Chrome, open the website, and click the **Legacy Bridge** extension icon.

If the Hikvision system asks for its Web Components plug-in, obtain the correct version from your organization or the equipment vendor. The proprietary installer is not included in this repository.

## What the scripts do

- `build.ps1` compiles the Windows helper programs from the source code in `native/`.
- `install.ps1` installs the helper for the current Windows user and connects it to the extension.
- `uninstall.ps1` removes the helper and its registration.

## Privacy

The extension requests the `activeTab` and `nativeMessaging` permissions. When you click its button, it sends the current tab's web address to the Windows helper. The helper accepts only HTTP and HTTPS addresses. It does not read local files or send page contents.

## Limitations

- The website opens in a separate Windows window, not inside the Chrome tab.
- Compatibility depends on Windows and the website.
- Pop-ups are blocked.
- ActiveX, Java, and Silverlight are not included or guaranteed.
- MSHTML is an old browser engine. Use it only with trusted websites and follow your organization's security rules.
- This independent project is not affiliated with Hikvision.
