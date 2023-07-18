//using ScottPlot.SnapLogic;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;

//namespace SBA;

//public class Allocator
//{
//    private List<Ship> ships;
//    private Quay quay;

//    private int[] _s;
//    private int[] _b;
//    private int[] s
//    {
//        get { return _s; }
//        set
//        {
//            if (value == null)
//                throw new ArgumentNullException(nameof(value));

//            // Check for negative values
//            foreach (int element in value)
//            {
//                if (element < 0)
//                    throw new ArgumentException(
//                        "Array cannot contain negative values.",
//                        nameof(value)
//                    );
//            }

//            _s = value;
//        }
//    } // Zeitpunkt, an dem Schiff i am Quai anlegt
//    private int[] b
//    {
//        get { return _b; }
//        set
//        {
//            if (value == null)
//                throw new ArgumentNullException(nameof(value));

//            // Check for negative values
//            foreach (int element in value)
//            {
//                if (element < 0)
//                    throw new ArgumentException(
//                        "Array cannot contain negative values.",
//                        nameof(value)
//                    );
//            }

//            _b = value;
//        }
//    } // Liegeposition von Schiff i

//    // Liste der verbotenen Positionen für Schiff i
//    // Schlüssel: Anliegezeitpunkt, Wert: Liste der verbotenen Positionen (Liegeposition, Endposition)
//    private Dictionary<int, List<Tuple<int, int>>> forbiddenPositions;

//    // Liste der verbotenen Zeiten für Schiff i
//    // Schlüssel: Liegeposition, Wert: Liste der verbotenen Zeiten (Anliegezeitpunkt, Endzeitpunkt)
//    private Dictionary<int, List<Tuple<int, int>>> forbiddenTimes;

//    private List<Ship> sortedShips; // TODO:

//    private List<Ship> A; // Menge der zugeordneten Schiffe
//    private HashSet<int> ASet; // Menge der zugeordneten Schiffe (als Hashset für schnelle Überprüfung
//    private Dictionary<int, Tuple<int, int>> finalShips; //TODO:

//    public Allocator(List<Ship> ships, Quay quay)
//    {
//        this.ships = ships;
//        this.quay = quay;
//        s = new int[this.ships.Count];
//        _s = new int[this.ships.Count];
//        b = new int[this.ships.Count];
//        _b = new int[this.ships.Count];
//        forbiddenPositions = new Dictionary<int, List<Tuple<int, int>>>();
//        forbiddenTimes = new Dictionary<int, List<Tuple<int, int>>>();

//        sortedShips = Ship.DeepCopy(ships); // Erstelle Kopie der Schiffe
//        sortedShips.Sort((s1, s2) => s1.HandlingTime.CompareTo(s2.HandlingTime)); //Sortiere Schiffe nach Liegezeit

//        A = new List<Ship>();
//        ASet = new HashSet<int>();
//        finalShips = new Dictionary<int, Tuple<int, int>>();
//    }

//    public void AllocateShips()
//    {
//        // Alle Schiffe durchgehen
//        while (A.Count < ships.Count)
//        {
//            // Das aktuelle Schiff zuerst auf null setzen
//            Ship v = null;

//            // Das Schiff mit der kürzesten Liegezeit suchen, das noch nicht zugeordnet wurde
//            foreach (Ship ship in sortedShips)
//            {
//                // Wenn das Schiff noch nicht zugeordnet wurde
//                if (!ASet.Contains(ship.ID))
//                {
//                    v = ship; //Schiff mit kürzester Liegezeit gefunden
//                    break;
//                }
//            }
//            if (v != null)
//            {
//                s[v.ID] = int.MaxValue; // Zuerst die Anlegezeit auf eine unrealistisch große Zahl setzen
//                b[v.ID] = quay.Capacity - v.Size; // Schiff v liegt am weitesten rechts

//                // Das erste Schiff automatisch auf die Position und die Anlegezeit 0 setzen
//                if (A.Count == 0)
//                {
//                    s[v.ID] = 0;
//                    b[v.ID] = 0;
//                }

//                // Alle bereits zugeordneten Schiffe durchgehen
//                foreach (Ship assignedShip in A)
//                {
//                    // Wenn die Positionen der Schiffe sich überschneiden
//                    if (
//                        PosOverlap(
//                            b[assignedShip.ID],
//                            b[assignedShip.ID] + assignedShip.Size,
//                            b[v.ID],
//                            b[v.ID] + v.Size
//                        )
//                    )
//                    {
//                        // Die Anlegezeit des aktuellen Schiffes auf die Ablegezeit des zugeordneten Schiffes setzen
//                        s[v.ID] = s[assignedShip.ID] + assignedShip.HandlingTime;
//                        // Das aktuelle Schiff so weit wie möglich nach rechts positionieren
//                        b[v.ID] = quay.Capacity - v.Size;
//                        //continue;
//                    }
//                    // Wenn es keine Überschneidung gibt
//                    else
//                    {
//                        // Anlegezeit des aktuellen Schiffes auf die Anlegezeit des zugeordneten Schiffes setzen
//                        s[v.ID] = s[assignedShip.ID];

//                        // Wenn das aktuelle Schiff näher zum zugeordneten Schiff liegen kann
//                        if (b[assignedShip.ID] + assignedShip.Size <= b[v.ID])
//                        {
//                            // Das aktuelle Schiff so weit wie möglich nach links positionieren
//                            b[v.ID] = b[assignedShip.ID] + assignedShip.Size;
//                        }
//                    }

//                    Boolean shipWithSameStartTime = false; // Flag, ob ein Schiff zum gleichen Zeitpunkt anlegt
//                    // Alle zugeordneten Schiffe durchgehen
//                    foreach (Ship ship in A)
//                    {
//                        // Wenn das Schiff zum gleichen Zeitpunkt anlegt
//                        if (s[ship.ID] == s[v.ID])
//                        {
//                            shipWithSameStartTime = true; // Flag setzen
//                            break; // Schleife abbrechen
//                        }
//                    }
//                    // Wenn kein Schiff zum gleichen Zeitpunkt anlegt
//                    if (!shipWithSameStartTime)
//                    {
//                        b[v.ID] = 0; // Das Schiff am Anfang des Quays positionieren
//                    }

//                    // !!!!!!!MACHT SINN???
//                    // Wenn die neue Position des aktuellen Schiffes keine Überschneidungen hat
//                    //if (!forbiddenPositions.ContainsKey(s[v.ID]))
//                    //{
//                    //    break; // Schleife abbrechen und das Schiff zuweisen
//                    //}

//                    bool isPositionForbidden = false; // Flag, ob die Position verboten ist

//                    // Alle verbotenen Positionen durchgehen
//                    foreach (var kvp in forbiddenPositions)
//                    {
//                        // Wenn die Anlegezeit des aktuellen Schiffes nicht mit der Anlegezeit des verbotenen Schiffes übereinstimmt
//                        if (kvp.Key != s[v.ID])
//                        {
//                            continue; // Schleife abbrechen und nächste Position überprüfen
//                        }
//                        // Alle Intervalle der verbotenen Position durchgehen
//                        foreach (var interval in kvp.Value)
//                        {
//                            // Wenn die Position des aktuellen Schiffes in einem verbotenen Intervall liegt
//                            if (b[v.ID] >= interval.Item1 && b[v.ID] < interval.Item2)
//                            {
//                                isPositionForbidden = true; // Flag setzen
//                                break; // Schleife abbrechen
//                                //continue testen
//                            }
//                        }
//                        // Wenn die Position nicht verboten ist
//                        if (!isPositionForbidden)
//                        {
//                            break; // Schleife abbrechen
//                        }
//                    }
//                    // Wenn die Position nicht verboten ist
//                    if (!isPositionForbidden)
//                    {
//                        break; // Schleife abbrechen und zur Zuweisung übergehen
//                    }
//                    else // testen!!!!!
//                    {
//                        b[v.ID] = quay.Capacity - v.Size; // Schiff so weit wie möglich nach rechts positionieren
//                        //s[v.ID] = int.MaxValue; // Anlegezeit auf unrealistisch große Zahl setzen
//                        continue; // Schleife fortsetzen und nächstes Schiff überprüfen
//                    }
//                }
//            }
//            A.Add(v); // Schiff zuweisen
//            ASet.Add(v.ID); // Schiff als zugeordnet markieren

//            // Wenn es schon ein Schlüssel mit der Anlegezeit des aktuellen Schiffes gibt
//            if (forbiddenPositions.ContainsKey(s[v.ID]))
//            {
//                // Zu diesem Schlüssel das Intervall der Position des aktuellen Schiffes hinzufügen
//                forbiddenPositions[s[v.ID]].Add(Tuple.Create(b[v.ID], b[v.ID] + v.Size));
//            }
//            else
//            {
//                // Ansonsten einen neuen Schlüssel mit dem Intervall der Position des aktuellen Schiffes hinzufügen
//                forbiddenPositions.Add(
//                    s[v.ID],
//                    new List<Tuple<int, int>> { Tuple.Create(b[v.ID], b[v.ID] + v.Size) }
//                );
//            }
//            // Wenn es schon ein Schlüssel mit der Liegeposition des aktuellen Schiffes gibt
//            if (forbiddenTimes.ContainsKey(b[v.ID]))
//            {
//                // Zu diesem Schlüssel das Intervall der Anlegezeit des aktuellen Schiffes hinzufügen
//                forbiddenTimes[b[v.ID]].Add(Tuple.Create(s[v.ID], s[v.ID] + v.HandlingTime));
//            }
//            else
//            {
//                // Ansonsten einen neuen Schlüssel mit dem Intervall der Anlegezeit des aktuellen Schiffes hinzufügen
//                forbiddenTimes.Add(
//                    b[v.ID],
//                    new List<Tuple<int, int>> { Tuple.Create(s[v.ID], s[v.ID] + v.HandlingTime) }
//                );
//            }

//            // Das aktuelle Schif mit der Anlegezeit und Liegeposition in die Liste der zugeordneten Schiffe einfügen
//            finalShips.Add(v.ID, Tuple.Create(s[v.ID], b[v.ID]));
//        }
//        // Die Liste aufsteigen nach Anlegezeit und Liegeposition sortieren
//        finalShips.OrderBy(x => x.Value.Item1).ThenBy(x => x.Value.Item2);

//        ASet.Clear(); // Die Menge der zugeordneten Schiffe leeren

//        // Duplikate der Anlegezeiten und Liegepositionen erstellen
//        // Vllt unnötig
//        List<int> sList = DeepCopyArrayToList(s);
//        List<int> bList = DeepCopyArrayToList(b);
//        sList.Sort();

//        //// Alle zugeordneten und aufsteigend sortierten Schiffe durchgehen
//        //foreach (var ship in finalShips)
//        //{
//        //    // Alle Key Value Paare der verbotenen Positionen durchgehen
//        //    foreach (var intervals in forbiddenTimes)
//        //    {
//        //        // Wenn der Schlüssel nicht der Anlegezeit des aktuellen Schiffes entspricht
//        //        if (intervals.Key != b[ship.Key])
//        //        {
//        //            continue; // Schleife abbrechen und nächstes Key Value Paar überprüfen
//        //        }
//        //        // Alle Intervalle der verbotenen Position durchgehen
//        //        foreach (var interval in intervals.Value.ToList())
//        //        {
//        //            // Wenn das Intervall gehört zum aktuellen Schiff
//        //            if (
//        //                interval.Item1 == s[ship.Key]
//        //                && interval.Item2 == s[ship.Key] + ships[ship.Key].HandlingTime
//        //            )
//        //            {
//        //                // Intervall aus der Liste der verbotenen Positionen entfernen
//        //                intervals.Value.Remove(interval);
//        //            }
//        //        }
//        //    }
//        //    foreach (var intervals in forbiddenPositions)
//        //    {
//        //        // Wenn der Schlüssel nicht der Anlegezeit des aktuellen Schiffes entspricht
//        //        if (intervals.Key != s[ship.Key])
//        //        {
//        //            continue; // Schleife abbrechen und nächstes Key Value Paar überprüfen
//        //        }
//        //        // Alle Intervalle der verbotenen Position durchgehen
//        //        foreach (var interval in intervals.Value.ToList())
//        //        {
//        //            // Wenn das Intervall gehört zum aktuellen Schiff
//        //            if (
//        //                interval.Item1 == b[ship.Key]
//        //                && interval.Item2 == b[ship.Key] + ships[ship.Key].Size
//        //            )
//        //            {
//        //                // Intervall aus der Liste der verbotenen Positionen entfernen
//        //                intervals.Value.Remove(interval);
//        //            }
//        //        }
//        //    }
//        //    // Die frühste Anliegezeit des aktuellen Schiffes finden
//        //    s[ship.Key] = FindSoonestArrival(ships[ship.Key]);
//        //    // Die möglich weiteste Position des aktuellen Schiffes finden
//        //    b[ship.Key] = FindFurthestPosition(ships[ship.Key]);

//        //    // Wenn es schon ein Schlüssel mit der Anlegezeit des aktuellen Schiffes gibt
//        //    if (forbiddenTimes.ContainsKey(b[ship.Key]))
//        //    {
//        //        // Zu diesem Schlüssel das Intervall der Anlegezeit des aktuellen Schiffes hinzufügen
//        //        forbiddenTimes[b[ship.Key]].Add(
//        //            Tuple.Create(s[ship.Key], s[ship.Key] + ships[ship.Key].HandlingTime)
//        //        );
//        //    }
//        //    else
//        //    {
//        //        // Ansonsten einen neuen Schlüssel mit dem Intervall der Anlegezeit des aktuellen Schiffes hinzufügen
//        //        forbiddenTimes.Add(
//        //            b[ship.Key],
//        //            new List<Tuple<int, int>>
//        //            {
//        //                Tuple.Create(s[ship.Key], s[ship.Key] + ships[ship.Key].HandlingTime)
//        //            }
//        //        );
//        //    }
//        //    if (forbiddenPositions.ContainsKey(s[ship.Key]))
//        //    {
//        //        // Zu diesem Schlüssel das Intervall der Anlegezeit des aktuellen Schiffes hinzufügen
//        //        forbiddenPositions[s[ship.Key]].Add(
//        //            Tuple.Create(b[ship.Key], b[ship.Key] + ships[ship.Key].Size)
//        //        );
//        //    }
//        //    else
//        //    {
//        //        // Ansonsten einen neuen Schlüssel mit dem Intervall der Anlegezeit des aktuellen Schiffes hinzufügen
//        //        forbiddenPositions.Add(
//        //            s[ship.Key],
//        //            new List<Tuple<int, int>>
//        //            {
//        //                Tuple.Create(b[ship.Key], b[ship.Key] + ships[ship.Key].Size)
//        //            }
//        //        );
//        //    }
//        //}
//    }

//    // deprecated
//    private Boolean PositionOverlap(Ship v, Ship assignedShip)
//    {
//        if (
//            b[assignedShip.ID] == b[v.ID]
//            || (b[v.ID] < b[assignedShip.ID] && b[assignedShip.ID] < b[v.ID] + v.Size)
//            || (
//                b[v.ID] + v.Size > b[assignedShip.ID] + assignedShip.Size
//                && b[assignedShip.ID] + assignedShip.Size > b[v.ID]
//            )
//        )
//        {
//            return true;
//        }
//        return false;
//    }

//    /// <summary>
//    /// Checks if two Ships overlap.
//    /// </summary>
//    /// <param name="sp1">Starting point of ship 1.</param>
//    /// <param name="ep1">End point of ship 1.</param>
//    /// <param name="sp2">Starting point of ship 2.</param>
//    /// <param name="ep2">End point of ship 2.</param>
//    /// <returns>True, if an overlap was found.</returns>
//    public static Boolean PosOverlap(int sp1, int ep1, int sp2, int ep2)
//    {
//        if ((sp2 <= sp1 && sp1 < ep2) || (ep2 >= ep1 && ep1 > sp2))
//        {
//            return true;
//        }
//        return false;
//    }

//    // bitte nicht löschen
//    //public static Boolean TimeOverlap(int[] s, int[] b, Ship v, List<Ship> A) // Name ändern
//    //{
//    //    foreach (Ship conflictShip in A)
//    //    {
//    //        if (s[conflictShip.ID] > s[v.ID])
//    //        {
//    //            if (s[conflictShip.ID] < s[v.ID] + v.HandlingTime)
//    //            {
//    //                if (
//    //                    PosOverlap(
//    //                        b[conflictShip.ID],
//    //                        b[conflictShip.ID] + conflictShip.Size,
//    //                        b[v.ID],
//    //                        b[v.ID] + v.Size
//    //                    )
//    //                )
//    //                {
//    //                    return true;
//    //                }
//    //            }
//    //        }
//    //    }
//    //    return false;
//    //}

//    public static List<int> DeepCopyArrayToList(int[] originalArray)
//    {
//        int[] clonedArray = new int[originalArray.Length];
//        Array.Copy(originalArray, clonedArray, originalArray.Length);
//        List<int> newList = clonedArray.ToList();

//        return newList;
//    }

//    private int FindSoonestArrival(Ship v)
//    {
//        if (s[v.ID] == 0) // if ship arrives at 0
//            return 0; // return 0

//        List<int> departures = new();

//        departures.Add(s[v.ID]);
//        foreach (Ship aShip in A)
//        {
//            //if (s[aShip.ID] + aShip.HandlingTime < s[v.ID])
//            //if (PosOverlap(s[aShip.ID], s[aShip.ID] + aShip.Size, s[v.ID], s[v.ID] + v.Size))
//            //{
//            int departure = s[aShip.ID] + aShip.HandlingTime;
//            bool isForbidden = false;

//            foreach (var intervals in forbiddenTimes)
//            {
//                foreach (var interval in intervals.Value)
//                {
//                    if (
//                        interval.Item1 <= departure
//                        && departure < interval.Item2
//                        && PosOverlap(intervals.Key, quay.Capacity, b[v.ID], b[v.ID] + v.Size)
//                    )
//                    {
//                        //departure = interval.Item2;
//                        isForbidden = true;
//                        break;
//                    }
//                }
//            }

//            if (!isForbidden)
//            {
//                departures.Add(departure);
//            }
//            //}
//        }

//        if (departures.Count > 0)
//        {
//            return departures.Min();
//        }
//        else
//        {
//            return s[v.ID];
//        }
//    }

//    public int FindFurthestPosition(Ship v)
//    {
//        int furthestPositionInLine = quay.Capacity - v.Size; // take furthest possible position in current line
//        int newPosition = quay.Capacity - v.Size;
//        List<int> positionsInLine = new List<int>(); // for storing all unavailable positions in current line

//        if (b[v.ID] + v.Size == quay.Capacity) // if ship is already at the end of the quay
//            return b[v.ID]; // return current position
//        foreach (Ship ship in A)
//        {
//            if (s[ship.ID] == s[v.ID] && ship.ID != v.ID)
//            {
//                if (b[ship.ID] >= b[v.ID])
//                    positionsInLine.Add(b[ship.ID]);
//            }
//        }
//        if (positionsInLine.Count > 1)
//        {
//            positionsInLine.Sort();
//        }
//        newPosition =
//            (positionsInLine.Count > 0) ? positionsInLine[0] - v.Size : furthestPositionInLine;
//        foreach (var intervals in forbiddenTimes)
//        {
//            foreach (var interval in intervals.Value)
//            {
//                if (PosOverlap(interval.Item1, interval.Item2, s[v.ID], s[v.ID] + v.HandlingTime))
//                {
//                    int conflictingEndPosition = 0;
//                    foreach (Tuple<int, int> endPos in forbiddenPositions[interval.Item1])
//                    {
//                        //if (endPos.Item1 == interval.Item2)
//                        if (
//                            PosOverlap(
//                                endPos.Item1,
//                                endPos.Item2,
//                                newPosition,
//                                newPosition + v.Size
//                            )
//                        )
//                        {
//                            //conflictingEndPosition = endPos.Item2;
//                            conflictingEndPosition =
//                                endPos.Item2 > conflictingEndPosition
//                                    ? endPos.Item2
//                                    : conflictingEndPosition;
//                        }
//                    }
//                    if (conflictingEndPosition >= newPosition + v.Size)
//                    { // if the forbidden interval ends after the current position
//                        if (
//                            PosOverlap(
//                                intervals.Key,
//                                quay.Capacity,
//                                newPosition,
//                                newPosition + v.Size
//                            )
//                            && intervals.Key - v.Size >= 0
//                        )
//                        {
//                            newPosition = intervals.Key - v.Size;
//                        }
//                    }
//                }
//            }
//        }
//        return newPosition;
//    }

//    public List<Solution> GetSolution()
//    {
//        List<Solution> solutions = new List<Solution>();
//        foreach (Ship ship in A)
//        {
//            solutions.Add(
//                new Solution(
//                    ship.ID,
//                    s[ship.ID],
//                    s[ship.ID] + ship.HandlingTime,
//                    b[ship.ID],
//                    b[ship.ID] + ship.Size
//                )
//            );
//        }
//        return solutions;
//    }

//    public void PrintSolution()
//    {
//        Console.WriteLine("Solution:");
//        // Das Ergebnis ausgeben
//        foreach (Ship a in A)
//        {
//            Console.WriteLine(
//                "Schiff "
//                    + a.ID
//                    + " | AnlegeZeit: "
//                    + s[a.ID]
//                    + " | Bearbeitung: "
//                    + a.HandlingTime
//                    + " | AblegeZeit: "
//                    + (s[a.ID] + a.HandlingTime)
//                    + " | Liegeposition: "
//                    + b[a.ID]
//                    + " | Size: "
//                    + a.Size
//                    + " | EndPosition: "
//                    + (b[a.ID] + a.Size)
//            );
//        }
//    }
//}
