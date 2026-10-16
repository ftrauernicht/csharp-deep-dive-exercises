🇩🇪 Deutsch | 🇬🇧 [English](../en/04-your-own-dictionary.md)

[← Gleichheit und Sortierung](03-gleichheit-und-sortierung.md) · Weiter: [LINQ](05-linq.md)

# Aufgabe 4: Ein eigenes Dictionary

**Ziel:** die Klasse bauen, auf die du dich seit Jahren verlässt. Danach weißt
du, was zwischen `dictionary[key]` und der Antwort passiert.

## 4.1 `BasketDictionary<TKey, TValue>`

In `AnimalShelter.Core.Collections`. **Intern sind nur Arrays und `List<T>`
erlaubt**: kein `Dictionary`, kein `HashSet`, und keine Vererbung von diesen
Klassen.

Die Einträge werden auf Körbe verteilt. Der Korb ergibt sich aus dem Hashcode
des Schlüssels modulo der Anzahl der Körbe. Innerhalb eines Korbs wird mit
`Equals` verglichen.

```csharp
using System.Collections;   // für den nicht-generischen IEnumerator, siehe unten

public class BasketDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    where TKey : notnull
{
    private List<KeyValuePair<TKey, TValue>>[] baskets;

    public BasketDictionary(int initialCapacity = 16) { ... }

    public int Count { get; private set; }

    public void Add(TKey key, TValue value) { ... }
    public bool ContainsKey(TKey key) { ... }
    public bool TryGetValue(TKey key, out TValue value) { ... }
    public bool Remove(TKey key) { ... }

    public TValue this[TKey key]
    {
        get { ... }
        set { ... }
    }

    public IEnumerable<TKey> Keys { get { ... } }
    public IEnumerable<TValue> Values { get { ... } }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { ... }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

Drei Stellen verdienen einen Satz, weil sie über das hinausgehen, was du
bisher gebraucht hast. Die ersten beiden stehen im Gerüst, die dritte in der
Liste darunter:

- `where TKey : notnull` verlangt vom Aufrufer schon beim Übersetzen einen
  Schlüsseltyp, der kein `null` zulässt. Das ist eine Warnung und keine
  Garantie: über einen `object`-Umweg oder aus Code ohne Nullable-Analyse
  kommt trotzdem `null` an. Deshalb steht in der Liste unten zusätzlich die
  `ArgumentNullException`.
- Die Aufzählung steht zweimal da. `IEnumerator IEnumerable.GetEnumerator()`
  ist die alte, typlose Fassung aus der Zeit vor den Generics. Sie wird
  **explizit** implementiert, also ohne `public` und mit dem Interface vor dem
  Methodennamen, damit sie nicht in der öffentlichen Oberfläche auftaucht und
  niemand versehentlich `object` statt `KeyValuePair` zurückbekommt. Dafür
  brauchst du `using System.Collections;`.
- `EqualityComparer<TKey>.Default` statt `key.Equals(other)`: Es kommt mit
  `null` auf beiden Seiten klar, findet `IEquatable<T>` ohne Boxing, und bei
  [Z1](06-zusatzaufgaben.md) tauschst du genau dieses eine Objekt aus, um
  Gleichheit von außen zu bestimmen.

Eine Stelle wird dich ärgern. `TryGetValue` muss auf dem Fehlschlag-Pfad etwas
in `value` schreiben, und `value = default;` ist bei eingeschalteter
Nullable-Analyse eine `warning CS8601: Possible null reference assignment`. Das
übliche Mittel ist `value = default!;`: Das Ausrufezeichen sagt dem Compiler,
dass du es besser weißt. Hier weißt du es wirklich besser, denn wer `false`
zurückbekommt, liest `value` nicht.

Verhalten wie beim `Dictionary<TKey, TValue>` aus .NET:

- `Add` mit vorhandenem Schlüssel → `ArgumentException`
- Indexer-get mit unbekanntem Schlüssel → `KeyNotFoundException`
- Indexer-set fügt hinzu oder überschreibt; beim Überschreiben bleibt `Count`
  gleich, weil kein Eintrag dazugekommen ist
- `Remove` liefert `true` oder `false`
- ein `null`-Schlüssel → `ArgumentNullException`, ein `initialCapacity` von 0
  oder weniger → `ArgumentOutOfRangeException`
- `Keys` und `Values` sind eine **Sicht**, keine Kopie: Was nach dem Abholen
  der Eigenschaft eingefügt wird, taucht beim Aufzählen auf. Beide liefern die
  Einträge in derselben Reihenfolge, Position für Position
- Schlüsselvergleich über `EqualityComparer<TKey>.Default`

Für die Sicht brauchst du `yield return`. Ein Getter, der `yield return`
benutzt, läuft nicht in einem Zug durch: Jedes `yield return` gibt ein
Element heraus und hält an dieser Stelle an, und erst die Nachfrage nach dem
nächsten Element setzt ihn fort. Deshalb liest die Sicht die Körbe in dem
Moment, in dem jemand sie aufzählt. Ein `return new List<TKey>(...)` wäre
die Kopie, die hier gerade nicht gemeint ist. Warum das so funktioniert, ist
der Inhalt von Aufgabe 5; hier genügt die Regel.

Was hier **nicht** verlangt ist: zu merken, dass sich das Dictionary während
einer Aufzählung verändert hat. Das ist
[Zusatzaufgabe Z2](06-zusatzaufgaben.md), und dort ist die Frage, was überhaupt
als Änderung zählt, der eigentliche Inhalt.

**Geh testgetrieben vor: zuerst der Test, dann die Implementierung.** Bei dieser
Aufgabe zahlt es sich aus, weil jede der Methoden für sich klein ist und erst
zusammen etwas ergeben.

## 4.2 Vorhersage: die Sonderfälle

Leg im Testprojekt eine Hilfsklasse `FixedHashKey` an, deren `GetHashCode()`
einen im Konstruktor übergebenen Wert liefert. Damit lassen sich Kollisionen
bestellen statt abwarten.

Sie braucht dafür zweierlei: den Hashcode aus dem Konstruktor und eine eigene
Identität, über die `Equals` entscheidet. Ohne die zweite bleibt nur
Referenzgleichheit, und dann lässt sich der dritte Punkt unten gar nicht
prüfen: Drei verschiedene Schlüssel mit demselben Hashcode sind genau der
Fall, in dem dein Dictionary innerhalb eines Korbs `Equals` fragen muss.

Prüfe:

- ein Schlüssel mit Hashcode **-7**
- ein Schlüssel mit Hashcode **`int.MinValue`**
- drei verschiedene Schlüssel mit **gleichem** Hashcode: alle einzeln finden,
  alle einzeln entfernen
- ein `Animal` als Schlüssel, gesucht mit einem anderen Objekt mit gleicher
  Chipnummer

Der zweite Fall ist der interessante. Schreib deine Vorhersage auf, was bei
`int.MinValue` passiert, bevor du es laufen lässt.

Und wenn du gleich `Math.Abs(hash) % baskets.Length` schreiben wolltest: Das
ist der naheliegende Weg, und er wirft eine `OverflowException`.
`Math.Abs(int.MinValue)` hat kein Ergebnis, das in einen `int` passt: die
positive Entsprechung existiert nicht. Wie kommst du stattdessen an
einen nicht-negativen Index?

Ein negativer Rest ist übrigens genauso ein Problem: `-7 % 16` ist in C# `-7`,
und ein Array mag das nicht. Es gibt mehr als einen Weg, daraus einen gültigen
Index zu machen, und sie landen nicht alle im selben Korb. Das ist in Ordnung,
solange der Index im Bereich liegt und derselbe Schlüssel immer denselben
Korb trifft.

## 4.3 Wachsen

Übersteigt das Verhältnis von Einträgen zu Körben den Wert **0.75**, wird die
Anzahl der Körbe verdoppelt und alle Einträge werden neu verteilt. Geprüft wird
nach dem Einfügen, und zwar bei jedem Einfügen: über `Add` genauso wie über den
Indexer.

Schreib das Verhältnis nicht als `Count / baskets.Length` hin. Beides sind
`int`, C# rechnet dann ganzzahlig, und bei 13 Einträgen in 16 Körben kommt 0
heraus statt 0,8125. Dein Dictionary würde nie wachsen, und auffallen würde es
erst an der Laufzeit.

Tests:

- 1 000 Einträge einfügen, alle werden wiedergefunden
- die Anzahl der Körbe wächst

Für den zweiten Test brauchst du Einblick von außen. Mach eine `internal`
Property `BasketCount` sichtbar über

```csharp
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AnimalShelter.Tests")]
```

Das Attribut muss vor jeder Namespace-Deklaration stehen. Üblich ist eine
eigene Datei `AssemblyInfo.cs` im Projekt `AnimalShelter.Core`; der Kopf von
`BasketDictionary.cs` tut es auch.

Das ist der ehrliche Weg: Die Eigenschaft bleibt aus dem öffentlichen Vertrag
heraus, und die Tests kommen trotzdem heran.

Überleg, warum neu verteilt werden **muss** und es nicht reicht, das Array zu
vergrößern. Der Korbindex hängt an der Anzahl der Körbe; nach dem Verdoppeln
liegt fast jeder Schlüssel woanders.

## 4.4 Die Klasse `Shelter`

In `AnimalShelter.Core.Management`. Die Tiere werden intern in **deinem**
`BasketDictionary<string, Animal>` gespeichert, Schlüssel ist die Chipnummer.

```csharp
public class Shelter : IEnumerable<Animal>
{
    public Shelter(string name, int capacity) { ... }

    public string Name { get; }
    public int Capacity { get; }
    public int AnimalCount { get; }      // zählt, was im Dictionary liegt

    public void Admit(Animal animal) { ... }
    public Animal Adopt(string chipNumber) { ... }
    public Animal this[string chipNumber] { get { ... } }

    public IEnumerator<Animal> GetEnumerator() { ... }
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

- `Admit` mit bereits vorhandener Chipnummer → `InvalidOperationException`
- `Admit` bei vollem Tierheim → `InvalidOperationException`
- `Adopt` entfernt das Tier und gibt es zurück; unbekannte Chipnummer →
  `KeyNotFoundException`
- `shelter["276098100000001"]` liefert das Tier; eine unbekannte Chipnummer →
  `KeyNotFoundException`, denn der Rückgabetyp ist `Animal` und nicht `Animal?`

Ab hier trägt die Klasse aus 4.1 echte Last.

## Was du hier gelernt hast

- Wie ein Dictionary einen Schlüssel findet, und warum das in konstanter Zeit
  geht
- Kollisionen als Normalfall, nicht als Ausnahme
- Warum `Math.Abs` für einen Hashcode die falsche Wahl ist
- Dass der Rest einer negativen Zahl in C# negativ bleibt
- Dass zwei `int` geteilt wieder ein `int` sind, auch wenn links ein `double`
  steht
- Warum Wachsen ein Neuverteilen ist
- `InternalsVisibleTo`, um zu testen, was nicht öffentlich sein soll
- Eine selbstgebaute Datenstruktur unter einer Klasse, die sie benutzt
