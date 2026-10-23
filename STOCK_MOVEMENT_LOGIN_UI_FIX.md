# Stock Movement and Login UX update

## Stock Movement alerts
- Reworked Low on Stock, Out of Stock, Expiring Medicine, and Expired Stock into clean white cards with subtle borders, status count pills, consistent spacing, and more legible two-line medicine entries.
- Added a subtle status dot and separators for each list item; red states indicate out-of-stock/expired, amber states indicate low/expiring.
- Increased the alert area slightly to improve readability while retaining the existing four-column layout and expiry-window selector.
- Existing data bindings and transaction-history behavior remain unchanged.

## Login keyboard flow
- Pressing Enter in Username moves focus to Password without submitting.
- Pressing Enter in Password invokes the existing Sign In action.
- Keyboard handling suppresses the default Enter key so it does not submit the form prematurely.

## Validation
- ZIP archive integrity checked.
- Build/runtime testing requires Visual Studio and the .NET SDK on Windows; this environment cannot compile/run the WinForms app.
