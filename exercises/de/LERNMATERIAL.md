🇩🇪 Deutsch | 🇬🇧 [English](../en/LEARNING-MATERIAL.md)

[← Überblick](00-ueberblick.md)

# Lernmaterial

Diese Liste ist kurz, und zwar absichtlich. Zu jedem Thema aus den Aufgaben
gibt es hundert Videos; was fehlt, ist nie das Material, sondern die Zeit.
Deshalb steht hier pro Thema eine Quelle, nicht fünf.

**Lies die Quelle nach der Aufgabe, nicht vorher.** Wer die Antwort schon
kennt, kann die Vorhersage nicht mehr falsch beantworten, und damit ist der
Übungseffekt weg. Die Reihenfolge ist: versuchen, scheitern, nachlesen,
verstehen, warum es gescheitert ist.

## Microsoft Learn

Die Referenz. Kostenlos, aktuell, und bei Widersprüchen gilt sie gegen jedes
Video und jeden Blogbeitrag.

| Seite | Was dort steht | Aufgabe |
|---|---|---|
| [Polymorphism](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/object-oriented/polymorphism) | `virtual`, `override`, `abstract`, und wie die Laufzeit entscheidet, welche Methode läuft | 1 |
| [Knowing when to use override and new keywords](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/knowing-when-to-use-override-and-new-keywords) | der Unterschied, der in Aufgabe 1 die Vorhersage entscheidet | 1 |
| [How to define value equality for a type](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/statements-expressions-operators/how-to-define-value-equality-for-a-type) | die vollständige Liste dessen, was zu `Equals` gehört, samt Operatoren | 3 |
| [Object.GetHashCode — Remarks](https://learn.microsoft.com/en-us/dotnet/fundamentals/runtime-libraries/system-object-gethashcode) | der Vertrag zwischen `Equals` und `GetHashCode`, und warum ein Schlüssel unveränderlich sein muss | 3, 4 |
| [IComparable&lt;T&gt;](https://learn.microsoft.com/en-us/dotnet/api/system.icomparable-1) | was `CompareTo` zurückgeben darf, und die Zusage, die in 3.4 mit der Gleichheit kollidiert | 3 |
| [Indexers](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/indexers/) | `this[...]`, mehrere Indexer an einer Klasse, unterschieden am Typ | 4, Z6 |
| [Deferred execution and lazy evaluation](https://learn.microsoft.com/en-us/dotnet/standard/linq/deferred-execution-lazy-evaluation) | welche LINQ-Methode sofort rechnet und welche nicht | 5 |

Der Abschnitt **Remarks** unter `GetHashCode` ist eine Liste von Regeln, die
man sonst durch Schaden lernt. Von den sieben Seiten lohnt diese sich am
meisten; lies sie nach Aufgabe 3 noch einmal.

## Videos

Fünf Stück, eines je Thema, zusammen rund eine Stunde. Ausgewählt nach dem
kürzesten Weg zur einen Einsicht, die die Aufgabe braucht, nicht nach
Vollständigkeit.

| Video | Länge | Aufgabe |
|---|---|---|
| [I Cut My Code in Half After Adding Just One Virtual Method](https://www.youtube.com/watch?v=jgtPwXHTdHc) · Zoran Horvat | 13 min | 1, 2 |
| [Here Is When HashSet & Dictionary Call GetHashCode and Equals](https://www.youtube.com/watch?v=wO3Rg5dV9GI) · Zoran Horvat | 8 min | 3 |
| [HashMaps & Dictionaries, Explained Simply](https://www.youtube.com/watch?v=y11XNXi9dgs) · Nic Barker | 23 min | 4 |
| [The Ultimate Guide to yield return and IEnumerable](https://www.youtube.com/watch?v=2chM9iMeY7k) · Zoran Horvat | 8 min | 5 |
| [Record vs Class vs Struct - What Most Developers Get Wrong](https://www.youtube.com/watch?v=Zmz3rpPyunQ) · Zoran Horvat | 9 min | Z5 |

Das erste Video ist das wichtigste für Aufgabe 2, weil es dieselbe Bewegung
zeigt, die dort verlangt wird: eine Typprüfung verschwindet, weil der Typ selbst
die Antwort kennt.

Das dritte erklärt Hashtabellen ohne C#, am Konzept. Hast du Aufgabe 4 gebaut,
erkennst du darin deine eigenen Körbe. In dem Moment wird aus einem Dictionary
eine Datenstruktur statt einer Zeile Syntax.

## Bezahltes

[Dometrain](https://dometrain.com/) hat zu jedem Thema dieses Pakets einen
Kurs, und sie sind gut. Sie kosten aber Geld, also stehen hier nur die drei
Kapitel, die auf einer Aufgabe liegen:

- **Hands-On: Data Structures & Algorithms in C#** (Nick Chapsas), Kapitel 14
  "Hash Tables" — Hashfunktion, Kollisionen mit Chaining, Resize. Das ist
  Aufgabe 4 und Z3 als Video.
- **Hands-On: LINQ in C#** (Nick Chapsas), Kapitel 1 "Deferred vs Immediate
  Execution" und Kapitel 12 "Building LINQ from Scratch" — Aufgabe 5.
- **Mastering C#** (Sergey Teplyakov), Kapitel 3 "Equality Semantics" —
  Aufgabe 3, deutlich anspruchsvoller als der Rest dieser Liste.

Nötig ist nichts davon. Das Paket ist so gebaut, dass die Microsoft-Learn-Seiten
oben reichen.

## Was fehlt und warum

Zu **xUnit** steht hier nichts. Das Testframework ist Werkzeug, nicht Thema:
`[Fact]`, `Assert.Equal`, `Assert.Throws`. Mehr braucht keine dieser Aufgaben,
und was darüber hinaus nötig wird, lernt man im Moment, in dem man es braucht.
Wer gar nicht weiß, wie ein Testprojekt läuft, fängt beim `dotnet test` aus dem
[Überblick](00-ueberblick.md#loslegen) an und liest den Rest, sobald ein Test
etwas behauptet, das er nicht versteht.

Zu **Records** steht in der Tabelle oben genau ein Video, und es ist Z5
zugeordnet, nicht Aufgabe 3. Zu **Pattern Matching** und **Streams** steht hier
gar nichts. Diese Dinge lösen Probleme, die du erst siehst, nachdem du sie von
Hand gelöst hast. In dieser Reihenfolge gelesen sind sie eine Erleichterung; in
der anderen sind sie Syntax zum Auswendiglernen.
