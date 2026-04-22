# Prism

A puzzle game where you guide colored light beams to crystals using mirrors and prisms.

Built with **Unity 6 (URP)** as a portfolio prototype for the Rollic Summer Internship application.

---

## Gameplay

The player places mirrors, prisms, and color filters on a grid to redirect light beams from sources to target crystals. When two different colored beams meet at a combiner, they mix into a new color (Red + Blue = Magenta, etc.). Each level is a small puzzle with a limited inventory of pieces.

## Status

Currently in development (Day 1).

### Roadmap
- [x] Project setup, folder structure, color data system
- [x] GameManager (Singleton) + GameState
- [ ] Grid system
- [ ] Light source + beam rendering
- [ ] Mirror & prism placement
- [ ] Color mixing logic
- [ ] Crystal targets + level complete detection
- [ ] 8 levels with progressive difficulty
- [ ] Polish: VFX, DOTween animations, audio
- [ ] WebGL build

## Tech Notes

- **Architecture:** Single Responsibility folder structure under `_Project/`, namespaced as `Prism.*`
- **Patterns:** Singleton for managers, ScriptableObject for color data
- **Color Mixing:** Pure static function with C# 8 switch expression
- **Engine:** Unity 6 with URP (for 2D Lights and bloom support)
- **Tools:** DOTween for animations, Unity Input System for controls

## Build

WebGL build will be available once the prototype is complete.

---

*Developed by Sude — Computer Engineering, 3rd year.*
