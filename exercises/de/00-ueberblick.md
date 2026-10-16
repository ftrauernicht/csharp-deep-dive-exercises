🇩🇪 Deutsch | 🇬🇧 [English](../en/00-overview.md)

# Überblick

Ein Tierheim, fünf Aufgaben und sieben Zusatzaufgaben. Am Ende steht eine
Klassenbibliothek, in der ein Dictionary von Hand gebaut ist, Gleichheit das
bedeutet, was du festgelegt hast, und du gesehen hast, wann LINQ eigentlich
rechnet.

## Für wen das ist

Du kannst Klassen, Vererbung und Interfaces, du hast Generics schon benutzt und
ein Testframework mindestens einmal gesehen. Was dir fehlt, ist die Schicht
darunter, und die steckt in diesen zwölf Aufgaben.

Wenn du gerade erst anfängst, fang nicht hier an.

## Wie hier gearbeitet wird

**Die Spezifikation steht im Text, das Urteil fällt der Test.** Es gibt keine
Musterausgabe zum Abgleichen und keine Schritt-für-Schritt-Anleitung. Jede
Aufgabe beschreibt, was gelten soll; ob es gilt, beantwortet der Test, den du
schreibst.

Das ist kein Selbstzweck. Einen Test zu schreiben zwingt dich, die Aufgabe
vorher zu Ende zu lesen, und genau daran scheitern die meisten Lösungen.

### Vorhersage-Aufgaben

Einige Abschnitte sind mit **Vorhersage** überschrieben. Dort gilt eine
zusätzliche Regel, und sie ist der Kern des ganzen Pakets:

> Schreib deine Erwartung als Kommentar in den Test, **bevor** du ihn ausführst.

```csharp
// Vorhersage: 2 -- beide Objekte landen im Set, weil ...
Assert.Equal(?, set.Count);
```

Erst dann laufen lassen. Lag die Vorhersage daneben, hast du gerade etwas
gelernt; lag sie richtig, weißt du es jetzt sicher statt ungefähr. Wer den Test
erst schreibt, nachdem er die Antwort kennt, hat eine Zeile Code produziert und
sonst nichts.

Die Kommentare bleiben stehen, auch wenn sie falsch waren. Sie sind die
Mitschrift.

Manche Abschnitte wollen keine Zahl, sondern eine Begründung oder eine
Entscheidung. Die gehört an dieselbe Stelle, als Kommentar über den Test,
der sie festhält. Eine eigene Datei verlangt nur die Übersicht in 5.3, und
dort steht es dabei.

### Der Analysator wird dir dabei im Weg stehen

Das Testprojekt bringt die Analysatoren von xUnit mit, und die kennen die
Vorhersage-Regel nicht. Sobald die vorhergesagte Zahl 0 oder 1 ist, kommt

```
warning xUnit2013: Do not use Assert.Equal() to check for collection size.
                   Use Assert.Single instead.
```

und bei `Assert.True(...Contains(...))` entsprechend xUnit2012 oder xUnit2017.
Die Vorschläge sind für normalen Testcode richtig und hier falsch: `Assert.Single`
löscht genau die Zahl aus dem Test, um die es geht, und `Assert.Contains`
vergleicht mit dem Vergleicher von xUnit statt mit dem, den du gerade
untersuchst.

Der Ausweg ist eine lokale Variable:

```csharp
int count = set.Count;
// Vorhersage: 1 -- beide Objekte sind gleich, also bleibt eines übrig
Assert.Equal(1, count);
```

Der Analysator sieht dann keinen Sammlungszugriff mehr im Assert, und dein
Testcode sagt weiterhin, was er prüft. Dieselbe Bewegung hilft überall dort,
wo du zwei Wege vergleichen willst, die beide `Contains` heißen.

## Projektstruktur

```
AnimalShelter.slnx
├── src/AnimalShelter.Core/        Klassenbibliothek
└── tests/AnimalShelter.Tests/     Testprojekt
```

| Namespace | Inhalt |
|---|---|
| `AnimalShelter.Core.Animals` | Tierklassen, Interfaces, Futterplan |
| `AnimalShelter.Core.Clinic` | Tierarzt |
| `AnimalShelter.Core.Comparison` | Comparer-Klassen |
| `AnimalShelter.Core.Collections` | eigenes Dictionary |
| `AnimalShelter.Core.Management` | Klasse `Shelter` |
| `AnimalShelter.Core.Queries` | LINQ-Auswertungen |
| `AnimalShelter.Core.Adoption` | Adoptionsanfragen (Zusatzaufgabe) |

Das Testprojekt spiegelt diese Struktur: Tests zu `AnimalShelter.Core.Animals`
liegen in `AnimalShelter.Tests.Animals`, und so weiter.

Im Wurzelverzeichnis liegt das Gerüst: zwei Projekte, die bauen, und sonst
nichts. Keine Klasse ist vorgegeben. Welche Datei in welchen Ordner gehört,
steht in der Tabelle darüber, und die Ordner sind schon da. Dort arbeitest du.

## Loslegen

```
dotnet build
dotnet test
```

Beim ersten Mal läuft genau ein Test, und der prüft nichts. Er steht da, damit
du siehst, dass die Kette von der Projektdatei bis zur Konsole funktioniert,
bevor du den ersten Test schreibst. Danach darf er weg.

## Die Musterlösung

Unter `solution/` liegt eine vollständige Fassung: Implementierung **und**
Tests, inklusive der ausgefüllten Vorhersagen.

**Sieh dort nicht nach, bevor du eine Aufgabe versucht hast.** Bei einer
Implementierung kostet ein Blick dich die Übung. Bei einer Vorhersage kostet er
dich die ganze Aufgabe, denn die Antwort zu kennen ist genau das, was sie
wertlos macht.

Die Musterlösung wird in der CI gebaut und getestet. Jede Zahl, die dort in
einem Vorhersage-Test steht, ist damit eine gemessene Zusicherung und keine
Behauptung. Ändert .NET sein Verhalten, fällt es dort auf.

## Reihenfolge

Die Aufgaben bauen aufeinander auf und werden nacheinander bearbeitet.
Aufgabe 3 ändert `Animal`, Aufgabe 4 benutzt `Animal` als Schlüssel, Aufgabe 5
setzt auf allem auf.

1. [Die Tierfamilie](01-tierfamilie.md) — Vererbung, Polymorphie, abstrakte Methoden
2. [Tierarzt und Interfaces](02-tierarzt-und-interfaces.md) — Typprüfungen loswerden
3. [Gleichheit, Hashcode und Sortierung](03-gleichheit-und-sortierung.md) — `IEquatable`, `GetHashCode`, `IComparable`
4. [Ein eigenes Dictionary](04-eigenes-dictionary.md) — Körbe, Kollisionen, Wachstum
5. [LINQ](05-linq.md) — Auswertungen und verzögerte Ausführung
6. [Zusatzaufgaben](06-zusatzaufgaben.md) — sieben Stück, Z1 bis Z7

Zum Nachlernen: [Lernmaterial](LERNMATERIAL.md).
