

<p align="center">
<img  src="https://user-images.githubusercontent.com/62462701/166122420-1eefe791-e897-4551-8248-574a1a021dda.png">
</p>

# 🧮 OpenLoop


![GitHub tag (latest by date)](https://img.shields.io/github/v/tag/JohannHoepfner/OpenLoop?label=version)
[![CodeQL](https://github.com/JohannHoepfner/OpenLoop/actions/workflows/codeql-analysis.yml/badge.svg)](https://github.com/JohannHoepfner/OpenLoop/actions/workflows/codeql-analysis.yml)
![GitHub](https://img.shields.io/github/license/JohannHoepfner/OpenLoop)
![GitHub top language](https://img.shields.io/github/languages/top/JohannHoepfner/OpenLoop)

## 🌟 Überblick
- Einfache Umgebung zur Programmierung numerischer Simulationen
- ausgelegt für die Verwendung durch Schüler/-innen ohne Programmierkenntnisse im Physikunterricht

## 🚀 Nutzung
![image](https://user-images.githubusercontent.com/62462701/166156053-b4345d50-8004-421d-a21f-7df314e024bf.png)

Das Hauptfenster von OpenLoop ist in zwei nebeneinander angeordnete Bereiche geteilt.

Die linke Seite erlaubt es, ein Skript unterteilt in den Code, der zum Start des Programmes ausgeführt wird, und den Code, der wiederholt ausgeführt wird, zu schreiben.

Die rechte Seite des Fensters bietet Kontrollen zum Ausführen des Programmes, darunter die Anzahl der Iterationen des Loop-Codes und auch die Einstellung, welche Variablen für X-Achse bzw. Y-Achse genutzt werden.

### Syntax
Jede Programmzeile in OpenLoop beginnt mit einem Variablennamen und einem "=" dahinter. Der mathematische Term dahinter wird ausgewertet und die links genannte Variable damit überschrieben. 

Loop:
```
x=x+1
y=x^2
```
Start:
```
x=0
```
Zur Auswertung der Terme wird die Bibliothek [Flee](https://github.com/mparlak/Flee) genutzt.

## ⬇️ Installation
