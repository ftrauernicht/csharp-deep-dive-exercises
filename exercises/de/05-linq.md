🇩🇪 Deutsch | 🇬🇧 [English](../en/05-linq.md)

[← Ein eigenes Dictionary](04-eigenes-dictionary.md) · Weiter: [Zusatzaufgaben](06-zusatzaufgaben.md)

# Aufgabe 5: LINQ

**Ziel:** neun Auswertungen schreiben, und danach sieben Experimente machen,
nach denen du nie wieder rätst, wann eine LINQ-Abfrage eigentlich rechnet.

## 5.1 Auswertungen

In `AnimalShelter.Core.Queries` eine Klasse `ShelterReport`, die im Konstruktor
einen `Shelter` erhält. Alle Methoden in **Methodensyntax**.

| Methode | Rückgabe | Inhalt |
|---|---|---|
| `Cats()` | `IEnumerable<Cat>` | alle Katzen |
| `Kittens()` | `IEnumerable<Cat>` | Katzen unter 12 Monaten |
| `NamesSorted()` | `IEnumerable<string>` | erst die Tiere über die Ordnung aus 3.4 sortieren, dann die Namen nehmen |
| `TotalDailyFoodInKg()` | `double` | Summe |
| `CountBySpecies()` | `Dictionary<string, int>` | etwa `{"Cat": 5, "Dog": 2}`; eine Eigenschaft `Species` gibt es nicht, der Typname ist der Schlüssel |
| `NotFullyVaccinated()` | `IEnumerable<Animal>` | impfbare Tiere mit fehlender Impfung |
| `Oldest()` | `Animal?` | `null` bei leerem Tierheim |
| `HeaviestThree()` | `IEnumerable<Animal>` | die drei schwersten, bei weniger als drei Tieren eben weniger |
| `HasOutdoorCats()` | `bool` | `true`, sobald eine Katze Freigänger ist |

Tests für jede Methode, **jeweils auch mit leerem Tierheim**: ein Test je
Methode, und geprüft wird das neutrale Ergebnis, also leere Folge, `0.0`,
leeres Dictionary, `false`, `null`. Dazu ein Test, der eine LINQ-Abfrage
direkt auf einem `BasketDictionary` ausführt.

`ShelterReport` hält das Tierheim selbst, keine Kopie davon. Das ist keine
Nebensache: Experiment 4 in 5.2 hängt daran.

Sieh dir beim Schreiben die Rückgabetypen an. Einige Methoden liefern ein
Ergebnis, andere eine Abfrage. Zwischen diesen beiden Dingen liegt der
Rest dieser Aufgabe.

Eine Falle wartet bei `Oldest()`. Seit Aufgabe 3 ist `Animal` ein
`IComparable<Animal>`, also übersetzt `shelter.Max()` anstandslos. Geliefert
wird aber nicht das älteste Tier, sondern das letzte in der Ordnung aus 3.4,
also nach Namen. Sieh nach, was deine Zeile wirklich vergleicht.

### Eine Falle beim Testen

Die Reihenfolge, in der `Shelter` seine Tiere herausgibt, ist **nicht
festgelegt**. Sie folgt der Korbverteilung deines `BasketDictionary`, und die
folgt dem Hashcode der Chipnummer, und `string.GetHashCode()` liefert in .NET
pro Prozessstart andere Werte.

Ein Test wie

```csharp
Assert.Equal(["Mimi", "Rex"], report.Cats().Select(c => c.Name));
```

läuft deshalb heute durch und morgen nicht. Sortier vor dem Vergleich, oder
prüfe den Inhalt ohne die Reihenfolge.

Bei `NamesSorted()` ist die Reihenfolge dagegen Teil der Zusage und darf
geprüft werden: Die Ordnung aus 3.4 fällt bei gleichem Namen auf die
Chipnummer zurück, und die ist eindeutig.

Bei `HeaviestThree()` gilt das **nur, solange keine zwei Tiere gleich schwer
sind**. `OrderByDescending` ist stabil, hält also bei gleichem Gewicht die
Reihenfolge der Quelle ein, und die Quelle ist genau die unbestimmte
Korbreihenfolge von oben. Bei vier gleich schweren Tieren wechselt dann nicht
nur die Reihenfolge, sondern die Auswahl: Es sind von Lauf zu Lauf andere
drei. Wähl die Gewichte in deinen Testdaten verschieden, oder prüfe etwas,
das nicht an der Quelle hängt.

## 5.2 Vorhersage: verzögerte Ausführung

Eine eigene Testklasse `DeferredExecutionTests`, in `AnimalShelter.Tests.Queries`.
Sieben Experimente, zu jedem ein Test, und über jedem Assert deine Vorhersage
als Kommentar. Am Ende sind
alle Tests grün und prüfen das **tatsächliche** Verhalten.

### 1. Änderung nach der Abfrage

```csharp
List<Animal> animals = new() { cat1, dog1 };
IEnumerable<Animal> cats = animals.Where(a => a is Cat);
animals.Add(cat2);
// cats.Count() == ?
```

Dasselbe noch einmal mit `.Where(...).ToList()` direkt nach der Abfrage.

### 2. Anzahl der Auswertungen

```csharp
int calls = 0;
IEnumerable<string> names = animals.Select(a => { calls++; return a.Name; });
// calls == ?
int count = names.Count();
// calls == ?
string first = names.First();
// calls == ?
foreach (string n in names) { }
// calls == ?
```

Vier Vorhersagen, vier Asserts. Der Zähler läuft dabei durch, er wird zwischen
den Schritten nicht zurückgesetzt.

Die zweite Vorhersage ist die, bei der sich die Geister scheiden: Die Quelle
ist eine `List<Animal>`, deren Anzahl längst feststeht.

### 3. Zeitpunkt der Exception

Eine Abfrage mit `Select`, deren Lambda für ein bestimmtes Tier eine Exception
wirft.

`Assert.Throws` umschließt **genau die Stelle, an der die Exception tatsächlich
auftritt**, nicht die, an der die Abfrage gebaut wird. Zeig beides: dass das
Bauen nichts tut, und wo es knallt.

Bei einem `foreach` ist diese Stelle nicht eine Zeile im Rumpf, sondern der
Schleifenkopf, der das nächste Element holt. Die ganze Schleife gehört also in
das Lambda.

### 4. Adoption während der Schleife

```csharp
foreach (Animal kitten in report.Kittens())
{
    shelter.Adopt(kitten.ChipNumber);
}
```

Was passiert? Sag es voraus, dann führ es aus.

Prüf danach zwei Dinge, die über die Fehlermeldung hinausgehen: **Woher** kommt
die Exception, also welche Klasse sie wirft? Und in welchem Zustand ist das
Tierheim danach?

Die Meldung selbst verrät die Klasse nicht. `ex.TargetSite?.DeclaringType`
schon, und was dort steht, ist eine Überraschung für sich.

Reparier es mit `ToList()` und begründe die gewählte Stelle. Eine Stelle rettet
gar nichts, nämlich der Schleifenrumpf. Es gibt mehr als eine, die trägt. Sag,
was die tragenden gemeinsam haben.

### 5. Reihenfolge der Auswertung

```csharp
private static IEnumerable<Animal> WithLog(IEnumerable<Animal> source, List<string> log)
{
    foreach (Animal animal in source)
    {
        log.Add($"yield {animal.Name}");
        yield return animal;
    }
}
```

Die Methode gibt ihre Elemente mit `yield return` heraus. Das ist keine
Schleife, die durchläuft und am Ende eine Liste zurückgibt: Jedes
`yield return` hält mitten in der Methode an und gibt die Kontrolle an den
Aufrufer zurück, und beim nächsten Element läuft sie genau dort weiter. Was im
Log steht, zeigt also, wie weit jemand die Abfrage gezogen hat.

Wie viele Einträge stehen im Log nach

- `WithLog(animals, log).Where(...).First()`
- `WithLog(animals, log).OrderBy(a => a.Name).First()`
- `WithLog(animals, log).Take(2).ToList()`

Wähl dafür mindestens vier Tiere, und stell das gesuchte **nicht** an die
erste Stelle. Steht es vorn, holen alle drei Zeilen ein Element, und der
Vergleich zeigt nichts.

Die zweite Zeile ist die, die am meisten erklärt.

Und dann die Frage, die den ganzen Mechanismus offenlegt: Läuft bei
`WithLog(...).Select(...).Where(...)` zuerst **jedes** Element durch `Select`
und danach jedes durch `Where`, oder läuft jedes Element einzeln durch die
ganze Kette? Prüf es, indem auch `Select` und `Where` ins Log schreiben, und
vergleich die Reihenfolge der Einträge.

### 6. Eingefangene Variable

```csharp
int minAge = 12;
IEnumerable<Animal> old = animals.Where(a => a.AgeInMonths >= minAge);
minAge = 100;
// old.Count() == ?
```

### 7. `Distinct`

```csharp
Cat original = new Cat("Mimi",  "276098100000001", 36, 4.0);
Cat renamed  = new Cat("Minka", "276098100000001", 36, 4.0);
List<Animal> list = new() { original, renamed };
// list.Distinct().Count() == ?
```

Und mit auskommentiertem `GetHashCode()`? Die Antwort hängt an Aufgabe 3, und
hier zeigt sich, wozu die ganze Arbeit dort gut war.

Dasselbe Problem wie in 3.3: Du kannst `Animal` nicht kaputt lassen, weil am
Ende alle Tests grün sein sollen. Miss also erst mit auskommentiertem
`GetHashCode()`, bau `Animal` zurück, und entscheide dann, wie die Messung
erhalten bleibt. Eine kleine Stellvertreter-Klasse im Testprojekt, die `Equals`
überschreibt und `GetHashCode` nicht, hält das Ergebnis dauerhaft fest; sie
braucht dann ein `#pragma warning disable CS0659` und einen Kommentar, warum
die Warnung hier genau richtig ist.

## 5.3 Übersicht

Leg eine Datei `LINQ.md` neben deine Solution-Datei, also ins
Wurzelverzeichnis deiner Arbeitskopie. In welcher Sprache du sie schreibst,
ist deine Sache; sie ist für dich. Inhalt:

- eine Tabelle: mindestens sechs LINQ-Methoden mit **sofortiger** und sechs
  mit **verzögerter** Ausführung
- je ein Beispiel aus deinem eigenen Code, wo `ToList()` nötig ist und wo es
  überflüssig ist

Beim zweiten Punkt wird es schwierig. Ein überflüssiges `ToList()` ist keine
Stilfrage: Es materialisiert eine Liste, die niemand braucht, und versteckt
dabei, dass die Abfrage gar nicht gelaufen wäre.

## Was du hier gelernt hast

- Der Unterschied zwischen einer Abfrage und einem Ergebnis, am Rückgabetyp
  ablesbar
- Dass eine Abfrage ihre Quelle erst beim Aufzählen liest, und was das bedeutet,
  wenn sich die Quelle dazwischen ändert
- Wie oft ein Lambda läuft, gemessen statt vermutet
- Dass verzögert nicht strömend heißt
- Warum eine Sammlung zu ändern, während man sie aufzählt, drei Ebenen tiefer
  knallt als man schaut
- Wo `ToList()` hingehört, und wo es nur so aussieht
- Dass eine eingefangene Variable die Variable ist und nicht ihr Wert
- Dass eine Zusage über die Reihenfolge nur so weit trägt wie der Vergleich,
  der sie herstellt
