🇩🇪 Deutsch | 🇬🇧 [English](README.md)

# C# Deep Dive Exercises

Zwölf Aufgaben rund um ein Tierheim, für Leute, die C# schon schreiben können.
Nicht die Sprache ist das Thema, sondern die Schicht darunter: warum `Equals`
ohne `GetHashCode` ein Fehler ist, was ein `Dictionary` tut, während es einen
Schlüssel sucht, und wann eine LINQ-Abfrage eigentlich rechnet.

Am Ende steht eine Klassenbibliothek mit einem selbstgebauten Dictionary,
einer Gleichheit, die bedeutet, was du festgelegt hast, und einer Sammlung
Tests, die festhalten, was du über .NET herausgefunden hast.

## Für wen das ist

Du kannst Klassen, Vererbung und Interfaces, du hast Generics benutzt und ein
Testframework mindestens einmal gesehen. Wer gerade erst anfängt, fängt besser
im [csharp-oop-course](https://github.com/ftrauernicht/csharp-oop-course) an;
dieses Paket setzt dort auf, wo der Kurs aufhört.

## Die Regel, auf der alles aufbaut

Einige Abschnitte heißen **Vorhersage**. Dort gilt:

> Schreib deine Erwartung als Kommentar in den Test, **bevor** du ihn
> ausführst.

Die Kommentare bleiben stehen, auch die falschen. Sie sind die Mitschrift.
Warum diese Reihenfolge darüber entscheidet, ob eine Aufgabe etwas wert ist,
steht im [Überblick](exercises/de/00-ueberblick.md).

## Was hier liegt

| Ort | Inhalt |
|---|---|
| [`exercises/de/`](exercises/de/00-ueberblick.md) | die Aufgaben auf Deutsch |
| [`exercises/en/`](exercises/en/00-overview.md) | dieselben Aufgaben auf Englisch |
| `AnimalShelter.slnx` im Wurzelverzeichnis | das Gerüst: zwei Projekte, die bauen, und sonst nichts. Hier arbeitest du |
| [`solution/`](solution/) | die vollständige Musterlösung, Implementierung und Tests |

Fünf Hauptaufgaben bauen aufeinander auf, sieben Zusatzaufgaben hängen jeweils
an einer davon und sind untereinander unabhängig.

## Was du brauchst

| Werkzeug | Wofür |
|---|---|
| [.NET SDK 10](https://dotnet.microsoft.com/download) | baut und testet den Code |
| ein Editor deiner Wahl | Visual Studio, Rider und VS Code können alle drei, was hier gebraucht wird |

Mehr nicht. Keine Datenbank, kein Webserver, keine Pakete außer xUnit, und das
holt sich das Testprojekt selbst.

## Auf deinen Rechner holen

**Ohne Git:** auf <https://github.com/ftrauernicht/csharp-deep-dive-exercises>
den grünen **Code**-Knopf, dann **Download ZIP**, und das Archiv irgendwohin
entpacken.

**Mit Git:**

```
git clone https://github.com/ftrauernicht/csharp-deep-dive-exercises.git
```

## Loslegen

```
dotnet build
dotnet test
```

Beim ersten Mal läuft genau ein Test, und der prüft nichts. Er steht da, damit
du siehst, dass die Kette von der Projektdatei bis zur Konsole funktioniert,
bevor du den ersten Test schreibst. Danach darf er weg.

Lesen fängst du hier an: [Überblick](exercises/de/00-ueberblick.md).

## Die Musterlösung

Unter `solution/` liegt eine vollständige Fassung mit 240 Tests, inklusive der
ausgefüllten Vorhersagen. Sie wird in der CI gebaut und getestet, und zwar
zweimal hintereinander, weil `string.GetHashCode()` pro Prozess einen anderen
Wert liefert und ein Test, der sich auf eine Aufzählungsreihenfolge verlässt,
sonst erst beim nächsten Lauf auffällt.

**Sieh dort nicht nach, bevor du eine Aufgabe versucht hast.** Bei einer
Implementierung kostet ein Blick dich die Übung. Bei einer Vorhersage kostet er
dich die ganze Aufgabe, denn die Antwort zu kennen ist genau das, was sie
wertlos macht.

## Zwei Sprachen

Jede Aufgabe gibt es auf Deutsch und auf Englisch, als zwei Dateien mit
demselben Inhalt. Der Code und die Kommentare im Code sind immer englisch. Das
ist keine Nachlässigkeit, sondern die Gewohnheit, die im Beruf gilt: Quellcode
wird englisch geschrieben, unabhängig davon, welche Sprache das Team spricht.

## Woher das kommt

Die Aufgaben sind als Übungspaket für eine Auszubildende entstanden, nachdem
die Grundlagen saßen und die Frage aufkam, was eigentlich unter `Dictionary`
und LINQ passiert. Sie liegen hier öffentlich, weil die Antwort auf diese Frage
nicht azubispezifisch ist.

Geschwister-Repositories mit demselben Aufbau:
[javascript-course](https://github.com/ftrauernicht/javascript-course),
[csharp-oop-course](https://github.com/ftrauernicht/csharp-oop-course),
[java-games-course](https://github.com/ftrauernicht/java-games-course).

## Lizenz

[MIT](LICENSE). Benutz es, ändere es, gib es weiter.
