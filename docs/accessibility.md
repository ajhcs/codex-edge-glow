# Accessibility

Accessibility is an active project requirement, not a completed claim.

## Current behavior

- Settings and calibration controls use native WPF or Windows Forms keyboard-focusable controls.
- The island supports keyboard submission with Enter and dismissal with Escape after its reply field is open.
- Text is not required to understand the edge animation; the island provides a written completion state.
- Color is user-configurable and the island includes shapes/text rather than relying only on hue.
- Essential installation and configuration steps are documented as text rather than screenshots alone.

## Known gaps

- The parent-painted island send action needs a stronger custom accessibility surface.
- A dedicated reduced-motion/static-flash mode is not yet available.
- High-contrast mode and screen-reader announcements need broader validation.
- Settings sliders need explicit accessible-name verification across supported Windows versions.
- The animation can be visually prominent and should gain frequency/intensity safeguards informed by accessibility testing.

## Contributing accessibility findings

Include Windows accessibility settings, input method, assistive technology/version, expected announcement or focus behavior, and exact reproduction steps. Remove private task content from any capture. Accessibility fixes are welcome even when they do not change visual appearance.

