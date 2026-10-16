🇩🇪 Deutsch | 🇬🇧 [English](../en/03-equality-and-ordering.md)

[← Tierarzt und Interfaces](02-tierarzt-und-interfaces.md) · Weiter: [Ein eigenes Dictionary](04-eigenes-dictionary.md)

# Aufgabe 3: Gleichheit, Hashcode und Sortierung

**Ziel:** festlegen, wann zwei Tiere dasselbe Tier sind, und danach sehen, was
`HashSet` und `Dictionary` damit anstellen, wenn man es halb macht.

## 3.1 Vorhersage: der Ausgangszustand

Bevor irgendetwas implementiert ist:

```csharp
Cat a = new Cat("Mimi", "276098100000001", 36, 4.0);
Cat b = new Cat("Mimi", "276098100000001", 36, 4.0);

bool sameByOperator = a == b;
bool sameByEquals   = a.Equals(b);

HashSet<Animal> set = new() { a, b };
int count = set.Count;
```

Schreib den Test **jetzt**, mit deinen drei Vorhersagen als Kommentar, und
führ ihn aus. Passe ihn an, nachdem 3.2 steht, und lass die alten Kommentare
stehen.

## 3.2 `IEquatable<Animal>`

**Zwei Tiere sind gleich, wenn ihre `ChipNumber` gleich ist.** Der Name spielt
keine Rolle.

Implementiere in `Animal`:

- `IEquatable<Animal>` mit `Equals(Animal? other)`
- `override Equals(object? obj)`
- `override GetHashCode()`
- die Operatoren `==` und `!=`, die auch mit `null` auf beiden Seiten umgehen

**Eine Entscheidung triffst du selbst:** Sind eine `Cat` und ein `Dog` mit
derselben Chipnummer gleich? Beide Antworten sind vertretbar. Schreib die
gewählte auf, begründe sie in einem Satz im Code, und setz sie um.

Tests:

- reflexiv, symmetrisch, transitiv
- `a.Equals(null)` ist `false`
- `null == null`, `a == null`, `null == a`
- gleiche Objekte haben gleiche Hashcodes
- gleiche Chipnummer, anderer Name → gleich
- gleicher Name, andere Chipnummer → ungleich

## 3.3 Vorhersage: drei Hash-Experimente

Zu jedem Experiment ein Test. Danach den Code wieder in den richtigen Zustand
bringen.

**A: `GetHashCode()` auskommentieren.**
Wie viele Elemente hat ein `HashSet<Animal>` nach dem Einfügen von zwei Tieren
mit gleicher Chipnummer? Und findet `ContainsKey` in einem
`Dictionary<Animal, string>` ein Tier, wenn mit einem **anderen** Objekt mit
gleicher Chipnummer gesucht wird?

Dazu drei Zeilen, die dasselbe zu tun scheinen. Sag für jede voraus, was sie
liefert, bevor du sie laufen lässt:

```csharp
dictionary.ContainsKey(probe)
dictionary.Keys.Contains(probe)
dictionary.Keys.Any(key => key.Equals(probe))
```

Zwei davon liefern dasselbe, nicht das, was man beim Lesen erwartet. Die
dritte liefert etwas anderes. Nur eine der drei Zeilen vergleicht mit
`Equals`; die anderen beiden rechnen vorher einen Hashcode aus und sehen nur
in einen Korb. Welche welche ist, findest du
heraus, indem du nachsiehst, welchen Typ `Keys` eigentlich hat.

Bevor überhaupt etwas läuft, sagt der Compiler dir Bescheid, und zwar zweimal:

```
warning CS0659: 'Animal' overrides Object.Equals(object o) but does not override Object.GetHashCode()
warning CS0661: 'Animal' defines operator == or operator != but does not override Object.GetHashCode()
```

Die zweite kommt von den Operatoren, die du in 3.2 geschrieben hast. Lass beide
für dieses Experiment stehen und merk dir ihren Wortlaut.

Und jetzt die Stelle, an der es unangenehm wird: **Beim nächsten Build sind sie
weg.** MSBuild hält das Projekt für aktuell, ruft den Compiler gar nicht erst
auf und spielt deshalb auch die Warnungen nicht noch einmal ab. Wer sie beim
ersten Mal überscrollt, sieht sie nie wieder, solange er die Datei nicht
anfasst. Zurückholen kannst du sie mit `dotnet build --no-incremental`. Das ist
die ehrlichste Antwort auf die Frage, wie dieser Fehler in echten Code kommt:
.NET meldet ihn, einmal.

**B: `GetHashCode()` liefert immer `1`.**
Funktionieren `HashSet` und `Dictionary` noch korrekt? Füg danach 20 000 Tiere
in ein `HashSet<Animal>` ein und miss die Zeit mit `Stopwatch`, einmal mit
`return 1;` und einmal mit der richtigen Implementierung.

Die Messung ist ein Test wie jeder andere. Damit du die Zahlen auch siehst,
brauchst du `ITestOutputHelper` im Konstruktor der Testklasse und
`dotnet test --logger "console;verbosity=detailed"`; `Console.WriteLine`
schluckt der Testläufer. Und die 20 000 Tiere brauchen 20 000 gültige
Chipnummern, also genau fünfzehn Ziffern.

Miss, bevor du rätst. Eine Millisekundenzahl als Erwartung gilt auf deinem
Rechner und sonst nirgends; prüf stattdessen das Verhältnis.

Der Compiler sagt bei diesem Experiment übrigens gar nichts. Ein Hashcode, der
immer 1 liefert, erfüllt den Vertrag; er ist nur nutzlos. Warnungen fangen
Fehler, keine schlechten Entscheidungen.

**C: `ChipNumber` bekommt vorübergehend einen öffentlichen Setter.**
Ein Tier wird in ein `HashSet` eingefügt, danach wird seine Chipnummer
geändert. Was liefert `set.Contains(animal)`, wenn man dasselbe Objekt
übergibt?

Das Ergebnis ist der Grund, warum ein Schlüssel unveränderlich sein sollte.

Beim Zurückbauen fällt dir dieses Experiment vor die Füße: Sobald der Setter
wieder weg ist, **übersetzt der Test nicht mehr**, und bis du das auflöst,
läuft gar kein Test mehr. Entscheide, was aus ihm wird. Eine Möglichkeit, die
die Lektion am Leben lässt: Vorhersage und Messung bleiben als Kommentar
stehen, und geprüft wird ab jetzt, dass es den Setter nicht gibt.

> **Hinweis zur Musterlösung:** Dort kann `Animal` nicht kaputt sein, weil die
> CI das Gegenteil prüft. Die drei fehlerhaften Zustände stehen deshalb als
> kleine Stellvertreter-Klassen im Testprojekt, jede auf genau eine Art falsch.
> Für dich ist der Weg über `Animal` der richtige: du siehst den Fehler dann
> an der Klasse, mit der du die ganze Zeit arbeitest.

## 3.4 `IComparable<Animal>`

Auch das implementierst du in `Animal`.

- Sortierung nach `Name`, ohne Beachtung der Groß- und Kleinschreibung
- bei gleichem Namen nach `ChipNumber`
- `CompareTo(null)` liefert einen positiven Wert

Tests:

- `List<Animal>.Sort()` mit gemischten Tieren
- zwei Tiere mit gleichem Namen
- `a.CompareTo(b)` und `b.CompareTo(a)` haben entgegengesetzte Vorzeichen
- **`a.CompareTo(b) == 0` genau dann, wenn `a.Equals(b)`**

### Der Widerspruch, den du gleich finden wirst

Die letzte Forderung passt nicht zu dem, was darüber steht, und das ist Absicht.

Nimm zwei Tiere mit **gleicher Chipnummer und verschiedenem Namen**. Nach 3.2
sind sie gleich. Nach der Sortierregel oben vergleicht `CompareTo` zuerst die
Namen, und die sind verschieden, also kommt etwas anderes als 0 heraus. Die
Forderung ist so nicht erfüllbar.

Lös es, bevor du weiterliest. Die Lösung ist eine Zeile, und sie steht ganz am
Anfang von `CompareTo`.

Das ist kein Fehler in der Aufgabe, sondern die Aufgabe. Zwei Vergleichsregeln,
die unabhängig voneinander vernünftig aussehen, widersprechen sich, sobald man
sie nebeneinanderlegt. Das merkt man nicht beim Lesen, sondern beim
Schreiben des Tests. Genau deshalb steht der Test in der Liste.

### Der Preis dieser Lösung

Die eine Zeile erfüllt die Forderung und bricht dabei eine andere, die niemand
aufgeschrieben hat. Nimm drei Tiere:

| | Name | Chipnummer |
|---|---|---|
| a | Anna | ...001 |
| b | Zoe | ...001 |
| c | Mia | ...002 |

a und b haben dieselbe Chipnummer, sind also gleich, und `a.CompareTo(b)`
liefert 0. Anna steht vor Mia, also ist a kleiner als c. Mia steht vor Zoe,
also ist c kleiner als b. Damit ist a gleich b und gleichzeitig kleiner als
etwas, das kleiner als b ist. Eine Ordnung ist das nicht mehr.

Nachgemessen beschwert sich `List<Animal>.Sort()` darüber nicht. Dieselben drei
Tiere kommen in unterschiedlicher Eingabereihenfolge unterschiedlich heraus:
`[a, b, c]` wird zu Anna, Mia, Zoe, und `[b, a, c]` wird zu Mia, Anna, Zoe. Das
zweite Ergebnis ist nicht einmal nach Namen sortiert.

Im Großen bleibt es dabei: Nimm ein paar tausend Tiere, bei denen jede
Chipnummer zweimal vorkommt, misch sie wiederholt und sortiere jedes Mal neu.
Fast jede neue Eingabereihenfolge liefert ein neues Ergebnis. Wie viele genau
herauskommen, hängt von den Namen und der Mischung ab; dass es mehr als eines
ist, hängt nur am Vergleich. Der Fehler meldet sich nie, er wirkt nur.

Das Paar, das den Widerspruch auslöst, sind zwei Tiere mit **derselben
Chipnummer und verschiedenen Namen**. Eine Chipnummer ist eindeutig, dieses
Paar kann es also gar nicht geben. Du hast drei Möglichkeiten, und alle drei
sind vertretbar:

- `Equals` zuerst abfragen und die Annahme danebenschreiben, dass Chipnummern
  eindeutig sind
- die Namen aus `CompareTo` werfen und nur nach Chipnummer sortieren: eine
  saubere Ordnung, aber eine Sortierung, die kein Mensch lesen will
- beim Namen bleiben und die Forderung aus der Testliste fallen lassen; dann
  gilt nur noch, dass gleiche Tiere 0 liefern, nicht die Umkehrung

Die Musterlösung nimmt die erste und schreibt die Annahme in den Kommentar.
Welche du nimmst, ist weniger wichtig als dass du aufschreibst, was du dafür
aufgegeben hast. Schreib dafür auch einen Test. Er hält die Entscheidung fest
und zwingt den Nächsten, sie erneut zu treffen, statt sie zu entdecken.

Optional: die Operatoren `<`, `>`, `<=`, `>=`.

## 3.5 `IComparer<Animal>`

In `AnimalShelter.Core.Comparison`:

- `ByAgeComparer`: jüngste zuerst
- `ByDailyFoodComparer`: höchster Futterbedarf zuerst

Teste beide mit `list.Sort(new ByAgeComparer())`.

Überleg dabei, warum es diese Klassen überhaupt gibt, wo `Animal` doch schon
`IComparable` ist. Eine Klasse hat **eine** natürliche Ordnung; jede weitere
braucht einen eigenen Vergleicher, der von außen kommt.

## Was du hier gelernt hast

- Gleichheit als Entscheidung, nicht als Vorgabe
- Warum `Equals` ohne `GetHashCode` nicht auffällt, bis ein `HashSet` ins Spiel
  kommt
- Dass ein konstanter Hashcode korrekt und trotzdem unbrauchbar ist
- Warum ein Schlüssel unveränderlich sein muss
- Dass `CompareTo` und `Equals` zusammenpassen müssen, und was passiert, wenn
  zwei Regeln sich im Weg stehen
- Dass eine Lösung einen Preis haben kann, der nirgends als Fehler auftaucht,
  sondern nur als ein Ergebnis, das von der Eingabereihenfolge abhängt
- Der Unterschied zwischen der einen natürlichen Ordnung und beliebig vielen
  weiteren
