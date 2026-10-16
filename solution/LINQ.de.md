🇩🇪 Deutsch | 🇬🇧 [English](LINQ.md)

# LINQ: sofort oder verzögert

Die Antwort auf [Aufgabe 5.3](../exercises/de/05-linq.md#53-übersicht). Alle
Zahlen hier sind gemessen, nicht nachgeschlagen, auf .NET 10 mit diesem
Zähler als Quelle:

```csharp
static IEnumerable<int> Source(List<string> log, int n)
{
    for (int i = 1; i <= n; i++)
    {
        log.Add($"pull {i}");
        yield return i;
    }
}
```

Die Quelle liefert fünf Zahlen und schreibt mitten im Liefern mit. Danach steht
im Protokoll, wie viele davon geholt wurden.

## Sofort: rechnet, wenn die Zeile läuft

| Methode | gezogen von 5 |
|---|---|
| `ToList()` | 5 |
| `ToArray()` | 5 |
| `ToDictionary(...)` | 5 |
| `ToHashSet()` | 5 |
| `Count()` | 5 |
| `Sum()`, `Max()`, `Average()` | 5 |
| `Last()` | 5 |
| `Aggregate(...)` | 5 |
| `First()` | 1 |
| `Any()` | 1 |
| `Any(i => i == 3)` | 3 |
| `All(i => i < 2)` | 2 |
| `Contains(2)` | 2 |
| `ElementAt(1)` | 2 |

Sofort heißt nicht vollständig. Die untere Hälfte der Tabelle rechnet genauso
sofort, hört aber auf, sobald die Antwort feststeht: `Any()` braucht ein
Element, `All` bricht beim ersten Gegenbeispiel ab.

Eine Ausnahme: `Single(i => i == 1)` zieht alle fünf, obwohl das passende
Element das erste war. Es muss beweisen, dass es kein
zweites gibt, und das geht nur bis zum Ende.

## Verzögert: baut nur die Abfrage

| Methode | nach dem Bauen | nach dem ersten `MoveNext()` |
|---|---|---|
| `Where(...)` | 0 | 1 |
| `Select(...)` | 0 | 1 |
| `OfType<T>()`, `Cast<T>()` | 0 | 1 |
| `Distinct()` | 0 | 1 |
| `Take(2)` | 0 | 1 |
| `Skip(1)` | 0 | **2** |
| `Except(...)` | 0 | 1 |
| `Concat(...)` | 0 | 1 |
| `SelectMany(...)` | 0 | 1 |
| `OrderBy(...)`, `OrderByDescending(...)` | 0 | **5** |
| `GroupBy(...)` | 0 | **5** |
| `Reverse()` | 0 | **5** |

Die erste Spalte ist überall 0, und das ist der ganze Punkt: Eine Abfrage zu
bauen liest die Quelle nicht an.

`Skip` ist die eine Zeile, deren Antwort nicht 1 ist. Es muss das Element, das
es überspringt, trotzdem holen, bevor es das nächste herausgeben kann.

Die zweite Spalte trennt die Gruppe in zwei. Oben wird **strömend** gearbeitet:
ein Element rein, ein Element raus. Unten wird **gepuffert**: Sortieren kann
das kleinste Element erst nennen, wenn es alle gesehen hat, und Gruppieren und
Umdrehen genauso. Verzögert und strömend sind also zwei verschiedene Zusagen,
und nur die erste gilt für alle zwölf Zeilen.

## Zwei Stellen, an denen sich Leute verschätzen

**`Count()` führt das Lambda aus.** Auch dann, wenn die Quelle ihre Anzahl längst
kennt:

```csharp
List<int> list = [1, 2, 3];
int calls = 0;
IEnumerable<int> query = list.Select(i => { calls++; return i; });

int count = query.Count();   // 3
// calls == 3
```

`list.Count` wäre 3 ohne jeden Aufruf. Durch das `Select` ist die Quelle aber
keine Liste mehr, sondern eine Abfrage, und eine Abfrage weiß ihre Länge erst,
wenn sie gelaufen ist. Wer im Lambda etwas tut, was mehr kostet als eine
Zuweisung, bezahlt es bei jedem `Count()`.

**Dieselbe Abfrage zweimal aufzählen heißt zweimal rechnen.** Eine `IEnumerable`
ist kein Ergebnis, das herumliegt, sondern eine Anweisung, die jedes Mal von
vorn ausgeführt wird. Zwei `foreach` über dieselbe Variable sind zwei Läufe.

## `ToList()`: hier nötig

In `DeferredExecutionTests.Kittens_MaterialisedBeforeTheForeach_AdoptsAllOfThem`:

```csharp
foreach (Animal kitten in report.Kittens().ToList())
{
    shelter.Adopt(kitten.ChipNumber);
}
```

Ohne `ToList()` wirft diese Schleife eine `InvalidOperationException`, und zwar
aus dem `BasketDictionary` heraus, das drei Ebenen unter dem `foreach` liegt.
`Kittens()` liest das Tierheim, während `Adopt` es verändert.

Die Stelle ist die ganze Lösung. `ToList()` steht **auf der Abfrage**, nicht im
Schleifenrumpf: Es liest das Tierheim fertig und schließt den Enumerator, bevor
die erste Adoption läuft. Im Rumpf würde es nichts retten, weil der Enumerator
der Schleife selbst noch offen wäre.

Nachgemessen stand das Tierheim beim Fehlschlag bei zwei von drei Tieren. Nicht
nichts getan und nicht fertig, sondern halb verändert. Das ist der Grund, warum
dieser Fehler in echtem Code so unangenehm ist.

## `ToList()`: hier überflüssig

So stand es in `ShelterReportTests`, bis die Aufgabe 5.3 es aufgedeckt hat:

```csharp
Assert.Equal(new[] { "Mimi", "Hoppel" }, report.HeaviestThree().Select(a => a.Name).ToList());
```

`Assert.Equal` vergleicht zwei `IEnumerable` selbst, Element für Element. Das
`ToList()` baut eine Liste, die nur entsteht, um sofort einmal gelesen und
weggeworfen zu werden. Es stand aus Gewohnheit da, nicht aus einem Grund, und
ist jetzt weg:

```csharp
Assert.Equal(new[] { "Mimi", "Hoppel" }, report.HeaviestThree().Select(a => a.Name));
```

Ein `ToList()` an einer solchen Stelle verdeckt, ob die Abfrage überhaupt
gelaufen wäre, und genau diese Frage ist in Aufgabe 5 der Unterschied zwischen
Verstehen und Raten.

## Und einer, der nötig aussieht und es nicht ist

In `Veterinarian.VaccinateAllMissing`:

```csharp
IEnumerable<string> missing = vaccinatable.RequiredVaccinations
    .Except(vaccinatable.ReceivedVaccinations)
    .ToList();

foreach (string vaccination in missing)
{
    vaccinatable.Vaccinate(vaccination);   // ändert ReceivedVaccinations
}
```

Das sieht aus wie der Fall von oben: Die Schleife verändert eine Sammlung, die
die Abfrage benutzt. Nachgestellt wirft es trotzdem nicht, auch ohne
`ToList()`. `Except` liest sein **zweites** Argument vollständig in ein Set,
bevor es das erste Element liefert, und danach ist kein Enumerator auf
`ReceivedVaccinations` mehr offen.

Das `ToList()` bleibt dort trotzdem stehen, und der Kommentar im Code sagt
warum: Dieses Verhalten ist eine Eigenschaft der heutigen Umsetzung von
`Except` und keine zugesagte Eigenschaft von LINQ. Eine Schleife, die eine
Quelle verändert, soll nicht davon abhängen.

Der Unterschied zum Abschnitt davor ist der Grund, warum diese Datei länger ist
als eine Tabelle: Ein überflüssiges `ToList()` kostet eine Liste, ein fehlendes
kostet einen halb durchgeführten Vorgang.
