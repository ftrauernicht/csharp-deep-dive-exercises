🇩🇪 Deutsch | 🇬🇧 [English](../en/02-vet-and-interfaces.md)

[← Die Tierfamilie](01-tierfamilie.md) · Weiter: [Gleichheit und Sortierung](03-gleichheit-und-sortierung.md)

# Aufgabe 2: Tierarzt und Interfaces

**Ziel:** eine Methode voller Typprüfungen in eine Methode ohne eine einzige
verwandeln, ohne dass ein Test sich ändert. Und danach die Frage beantworten,
wann eine Typprüfung trotzdem in Ordnung ist.

## 2.1 Den Tierarzt umbauen

Leg diese Klasse in `AnimalShelter.Core.Clinic` an, so wie sie dasteht:

```csharp
public class Veterinarian
{
    public decimal CalculateTreatmentCost(Animal animal)
    {
        if (animal is Cat)
        {
            return 45.00m;
        }
        else if (animal is Dog dog)
        {
            return dog.WeightInKg > 20 ? 70.00m : 55.00m;
        }
        else if (animal is Rabbit)
        {
            return 30.00m;
        }

        throw new NotSupportedException($"Unknown animal: {animal.GetType().Name}");
    }
}
```

**Erster Schritt: Tests für das aktuelle Verhalten.** Katze, Hund mit 15 kg,
Hund mit **genau 20 kg**, Hund mit 25 kg, Kaninchen.

Die 20 kg sind der Fall, der später beweist, dass du nichts kaputt gemacht
hast. Die Bedingung lautet `> 20`, bei genau zwanzig Kilogramm gilt also der
niedrigere Preis. Wer beim Umbau versehentlich `>=` schreibt, merkt es an
diesem einen Test.

**Zweiter Schritt: umbauen.** Danach enthält `CalculateTreatmentCost` keine
Typprüfung mehr: Der Preis kommt vom Tier selbst, als neue Eigenschaft an
`Animal`, und der Tierarzt fragt sie nur noch ab. Wie du sie nennst, bleibt
dir überlassen. **Die Tests aus Schritt 1 bleiben unverändert und grün.** Wenn
du einen Test anfassen musst, war es kein Umbau, sondern eine Änderung.

**Dritter Schritt: eine neue Tierart.** Füg `Budgie` hinzu: Wellensittich,
`"Tweet"`, 10 g Futter pro Tag unabhängig vom Gewicht, Behandlung 25 €.

`Veterinarian` wird dafür **nicht** angefasst. Das ist die Probe aufs Exempel:
Wäre die Typprüfung noch da, müsstest du jetzt dort hinein, und das alte
`throw new NotSupportedException` hätte dir vorher nichts gesagt. Aufgefallen
wäre es erst zur Laufzeit, beim ersten Wellensittich.

Überleg beim Umbau, ob die neue Eigenschaft an `Animal` `virtual` oder
`abstract` sein soll. Beides geht; nur eines zwingt den nächsten, der eine
Tierart hinzufügt, über den Preis nachzudenken.

## 2.2 Das Interface `IVaccinatable`

```csharp
public interface IVaccinatable
{
    IReadOnlyList<string> RequiredVaccinations { get; }
    IReadOnlyCollection<string> ReceivedVaccinations { get; }
    void Vaccinate(string vaccination);
    bool IsFullyVaccinated { get; }
}
```

Die beiden Rückgabetypen sind verschieden, und das ist Absicht. Die
Pflichtimpfungen haben eine Reihenfolge, die unten in der Tabelle steht. Die
bereits gegebenen haben keine, weil dieselbe Impfung zweimal gegeben einmal
zählt. `IReadOnlyCollection` sagt genau das: abzählbar, aber ohne Index, auf
den sich jemand verlassen könnte.

| Tier | Pflichtimpfungen |
|---|---|
| `Cat` | `"CatFlu"`, `"Panleukopenia"` |
| `Dog` | `"Distemper"`, `"Parvovirus"`, `"Rabies"` |
| `Rabbit` | `"RHD"`, `"Myxomatosis"` |
| `Budgie` | nicht impfbar |

Regeln:

- Eine Impfung, die nicht zu den Pflichtimpfungen gehört, wirft eine
  `ArgumentException`.
- Dieselbe Impfung zweimal zu geben ist erlaubt, sie zählt aber nur einmal.
- **Die Impflogik existiert nur an einer Stelle im Code.** Drei Tierarten mit
  derselben Logik heißt nicht dreimal dieselbe Logik. Eine Klasse, die die
  Tiere intern benutzen — etwa `VaccinationRecord` — reicht dafür.

`Cat`, `Dog` und `Rabbit` implementieren das Interface. `Budgie` bleibt
absichtlich außen vor, und das ist die Voraussetzung für 2.3.

## 2.3 Alle impfen

Noch eine Methode an `Veterinarian`:

```csharp
public int VaccinateAllMissing(IEnumerable<Animal> animals)
```

Gibt jedem impfbaren Tier alle fehlenden Pflichtimpfungen und liefert zurück,
wie viele Impfungen gesetzt wurden.

Tests: eine gemischte Liste inklusive Wellensittich, ein zweiter Aufruf direkt
danach (der 0 liefern muss), und ein Tier, das schon eine von zwei Impfungen
hat.

## Die beiden Fragen

**Warum ist `is IVaccinatable` hier vertretbar, `is Cat` in 2.1 aber nicht?**

Schreib die Antwort auf, bevor du weiterliest. Sie lässt sich in einem Satz
sagen.

Der Unterschied: `is Cat` fragte nach der Art, um eine Zahl nachzuschlagen, die
das Tier längst kennt: die Information war am falschen Ort. `is IVaccinatable`
fragt, ob das Tier etwas überhaupt **kann**, und darauf kann nur das Typsystem
antworten. Es gibt keinen sinnvollen Wert, den ein Wellensittich für
`Vaccinate` zurückgeben könnte.

**Wann wählst du eine abstrakte Klasse, wann ein Interface?**

Auch diese Antwort gehört aufgeschrieben. In diesem Projekt steht beides
nebeneinander: `Animal` ist abstrakt, `IVaccinatable` ist ein Interface. Warum
nicht umgekehrt?

## Was du hier gelernt hast

- Einen Umbau, der durch unveränderte Tests abgesichert ist, von einer Änderung
  unterscheiden
- Dass eine Typprüfung meist bedeutet, dass die Information am falschen Ort liegt
- `abstract` als Weg, die nächste Person zu einer Entscheidung zu zwingen,
  statt sie zur Laufzeit scheitern zu lassen
- Ein Interface als Aussage über eine Fähigkeit, nicht über eine Art
- Gemeinsame Logik in eine eigene Klasse ziehen, statt sie dreimal zu erben
