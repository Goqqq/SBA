using ScottPlot.SnapLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace SBA;

public class Allocator
{
    private List<Ship> ships;
    private Quay quay;

    private int[] _s;
    private int[] _b;
    private int[] s
    {
        get { return _s; }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Check for negative values
            foreach (int element in value)
            {
                if (element < 0)
                    throw new ArgumentException(
                        "Array cannot contain negative values.",
                        nameof(value)
                    );
            }

            _s = value;
        }
    } // Zeitpunkt, an dem Schiff i am Quai anlegt
    private int[] b
    {
        get { return _b; }
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Check for negative values
            foreach (int element in value)
            {
                if (element < 0)
                    throw new ArgumentException(
                        "Array cannot contain negative values.",
                        nameof(value)
                    );
            }

            _b = value;
        }
    } // Liegeposition von Schiff i

    // Liste der verbotenen Positionen für Schiff i
    // Schlüssel: Anliegezeitpunkt, Wert: Liste der verbotenen Positionen (Liegeposition, Endposition)
    private Dictionary<int, List<Tuple<int, int>>> forbiddenPositions;

    // Liste der verbotenen Zeiten für Schiff i
    // Schlüssel: Liegeposition, Wert: Liste der verbotenen Zeiten (Anliegezeitpunkt, Endzeitpunkt)
    private Dictionary<int, List<Tuple<int, int>>> forbiddenTimes;

    private List<Ship> sortedShips; // TODO:

    private List<Ship> A; // Menge der zugeordneten Schiffe
    private HashSet<int> ASet; // Menge der zugeordneten Schiffe (als Hashset für schnelle Überprüfung)
    private Dictionary<int, Tuple<int, int>> finalShips; // Schlüssel: Schiff-ID, Wert: (Anliegezeitpunkt, Liegeposition)
    private Benchmark allocateShipsBench;

    List<Benchmark> benchmarks;

    public Allocator(List<Ship> ships, Quay quay)
    {
        this.ships = ships;
        this.quay = quay;
        s = new int[this.ships.Count];
        _s = new int[this.ships.Count];
        b = new int[this.ships.Count];
        _b = new int[this.ships.Count];
        forbiddenPositions = new Dictionary<int, List<Tuple<int, int>>>();
        forbiddenTimes = new Dictionary<int, List<Tuple<int, int>>>();

        sortedShips = Ship.DeepCopy(ships); // Erstelle Kopie der Schiffe
        sortedShips.Sort((s1, s2) => s1.HandlingTime.CompareTo(s2.HandlingTime)); //Sortiere Schiffe nach Liegezeit

        A = new List<Ship>();
        ASet = new HashSet<int>();
        finalShips = new Dictionary<int, Tuple<int, int>>();

        allocateShipsBench = new Benchmark();
        benchmarks.Add(allocateShipsBench);
    }

    public void AllocateShips()
    {
        // Alle Schiffe durchgehen
        allocateShipsBench.Start();

        while (A.Count < ships.Count)
        {
            // Das aktuelle Schiff zuerst auf null setzen
            Ship v = null;
            bool newPosAssigned = false; // Flag, ob eine neue Position zugewiesen wurde
            // Das Schiff mit der kürzesten Liegezeit suchen, das noch nicht zugeordnet wurde
            foreach (Ship ship in sortedShips)
            {
                // Wenn das Schiff noch nicht zugeordnet wurde
                if (!ASet.Contains(ship.ID))
                {
                    v = ship; //Schiff mit kürzester Liegezeit gefunden
                    break;
                }
            }
            if (v != null)
            {
                s[v.ID] = int.MaxValue; // Zuerst die Anlegezeit auf eine unrealistisch große Zahl setzen
                b[v.ID] = quay.Capacity - v.Size; // Schiff v liegt am weitesten rechts

                // Das erste Schiff automatisch auf die Position und die Anlegezeit 0 setzen
                if (A.Count == 0)
                {
                    s[v.ID] = 0;
                    b[v.ID] = 0;
                }

                // Alle bereits zugeordneten Schiffe durchgehen
                foreach (Ship assignedShip in A)
                {
                    if (!newPosAssigned)
                    {
                        // Wenn die Positionen der Schiffe sich überschneiden
                        if (
                            PosOverlap(
                                b[assignedShip.ID],
                                b[assignedShip.ID] + assignedShip.Size,
                                b[v.ID],
                                b[v.ID] + v.Size
                            )
                        )
                        {
                            // Die Anlegezeit des aktuellen Schiffes auf die Ablegezeit des zugeordneten Schiffes setzen
                            s[v.ID] = s[assignedShip.ID] + assignedShip.HandlingTime;
                            // Das aktuelle Schiff so weit wie möglich nach rechts positionieren
                            //b[v.ID] = quay.Capacity - v.Size;
                            b[v.ID] = 0;
                        }
                        // Wenn es keine Überschneidung gibt
                        else
                        {
                            // Anlegezeit des aktuellen Schiffes auf die Anlegezeit des zugeordneten Schiffes setzen
                            s[v.ID] = s[assignedShip.ID];

                            // Wenn das aktuelle Schiff näher zum zugeordneten Schiff liegen kann
                            if (b[assignedShip.ID] + assignedShip.Size <= b[v.ID])
                            {
                                // Das aktuelle Schiff so weit wie möglich nach links positionieren
                                b[v.ID] = b[assignedShip.ID] + assignedShip.Size;
                                //b[v.ID] = 0;
                            }
                        }
                    }

                    // Alle verbotenen Positionen durchgehen
                    foreach (var kvpP in forbiddenPositions)
                    {
                        foreach (var intervalP in kvpP.Value)
                        // Alle intervale von verbotenen Positionen durchgehen
                        {
                            if (
                                PosOverlap(
                                    intervalP.Item1,
                                    intervalP.Item2,
                                    b[v.ID],
                                    b[v.ID] + v.Size
                                )
                            )
                            // Wenn es eine positionelle Überschneidung gibt
                            {
                                foreach (var kvpT in forbiddenTimes)
                                // Alle verbotenen Zeiten durchgehen
                                {
                                    foreach (var intervalT in kvpT.Value)
                                    // Alle verbotenen Intervale durchgehen
                                    {
                                        if (
                                            kvpP.Key == intervalT.Item1
                                            && kvpT.Key == intervalP.Item1
                                        )
                                        // Ein Schiff identifizieren
                                        {
                                            if (
                                                PosOverlap(
                                                    intervalT.Item1,
                                                    intervalT.Item2,
                                                    s[v.ID],
                                                    s[v.ID] + v.HandlingTime
                                                )
                                            )
                                            // Wenn es eine zeitliche Überschneidung gibt
                                            {
                                                newPosAssigned = true; // Flag setzen
                                                if (quay.Capacity < intervalP.Item2 + v.Size)
                                                // Wenn das Schiff nicht mehr auf das Quay passt
                                                {
                                                    s[v.ID] = intervalT.Item2; // Anlegezeit auf die Ablegezeit des Schiffes setzen
                                                    b[v.ID] = 0; // Schiff ganz links positionieren
                                                }
                                                else
                                                {
                                                    s[v.ID] =
                                                        intervalT.Item1 > s[v.ID] // Wenn die Anlegezeit des Schiffes größer ist als die Anlegezeit des aktuellen Schiffes
                                                            ? intervalT.Item1 // Anlegezeit des Schiffes setzen
                                                            : s[v.ID]; // Anlegezeit des aktuellen Schiffes beibehalten

                                                    b[v.ID] = intervalP.Item2; // Schiff v rechts nach dem überschneidenden Schiff positionieren
                                                }
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            A.Add(v); // Schiff zuweisen
            ASet.Add(v.ID); // Schiff als zugeordnet markieren

            // Die verbotenen Positionen und Zeiten setzen
            UpdateForbiddenIntervals(s[v.ID], Tuple.Create(b[v.ID], b[v.ID] + v.Size), "position");
            UpdateForbiddenIntervals(
                b[v.ID],
                Tuple.Create(s[v.ID], s[v.ID] + v.HandlingTime),
                "time"
            );
            // Das aktuelle Schif mit der Anlegezeit und Liegeposition in die Liste der zugeordneten Schiffe einfügen
            finalShips.Add(v.ID, Tuple.Create(s[v.ID], b[v.ID]));
        }

        // Die Liste aufsteigend nach Anlegezeit und Liegeposition sortieren
        finalShips.OrderBy(x => x.Value.Item1).ThenBy(x => x.Value.Item2);

        ASet.Clear(); // Die Menge der zugeordneten Schiffe leeren

        // Alle zugeordneten und aufsteigend sortierten Schiffe durchgehen
        foreach (var ship in finalShips)
        {
            RemoveForbiddenValues(ship.Key); // Verbotene Positionen und Zeiten von aktuellem Schiff entfernen

            OptimizePositions(ships[ship.Key]); // Position des aktuellen Schiffes optimieren

            // Die verbotenen Positionen und Zeiten setzen
            UpdateForbiddenIntervals(
                s[ship.Key],
                Tuple.Create(b[ship.Key], b[ship.Key] + ships[ship.Key].Size),
                "position"
            );
            UpdateForbiddenIntervals(
                b[ship.Key],
                Tuple.Create(s[ship.Key], s[ship.Key] + ships[ship.Key].HandlingTime),
                "time"
            );
        }
        allocateShipsBench.Stop();
        //foreach (var ship in finalShips)
        //{
        //    RemoveForbiddenValues(ship.Key); // Verbotene Positionen und Zeiten von aktuellem Schiff entfernen

        //    OptimizePositions(ships[ship.Key]); // Position des aktuellen Schiffes optimieren

        //    // Die verbotenen Positionen und Zeiten setzen
        //    UpdateForbiddenIntervals(
        //        s[ship.Key],
        //        Tuple.Create(b[ship.Key], b[ship.Key] + ships[ship.Key].Size),
        //        "position"
        //    );
        //    UpdateForbiddenIntervals(
        //        b[ship.Key],
        //        Tuple.Create(s[ship.Key], s[ship.Key] + ships[ship.Key].HandlingTime),
        //        "time"
        //    );
        //}
    }

    /// <summary> Entfernt die verbotenen Zeiten und Positionen vom Schiff </summary>
    /// <param name="shipID"> Die ID des Schiffes </param>
    private void RemoveForbiddenValues(int shipID)
    {
        foreach (var intervals in forbiddenTimes)
        {
            // Wenn der Schlüssel nicht der Anlegezeit des aktuellen Schiffes entspricht
            if (intervals.Key != b[shipID])
            {
                continue; // Schleife abbrechen und nächstes Key Value Paar überprüfen
            }
            // Alle Intervalle der verbotenen Position durchgehen
            foreach (var interval in intervals.Value.ToList())
            {
                // Wenn das Intervall zum aktuellen Schiff gehört
                if (
                    interval.Item1 == s[shipID]
                    && interval.Item2 == s[shipID] + ships[shipID].HandlingTime
                )
                {
                    // Intervall aus der Liste der verbotenen Positionen entfernen
                    intervals.Value.Remove(interval);
                }
            }
        }
        foreach (var intervals in forbiddenPositions)
        {
            // Wenn der Schlüssel nicht der Anlegezeit des aktuellen Schiffes entspricht
            if (intervals.Key != s[shipID])
            {
                continue; // Schleife abbrechen und nächstes Key Value Paar überprüfen
            }
            // Alle Intervalle der verbotenen Position durchgehen
            foreach (var interval in intervals.Value.ToList())
            {
                // Wenn das Intervall gehört zum aktuellen Schiff
                if (interval.Item1 == b[shipID] && interval.Item2 == b[shipID] + ships[shipID].Size)
                {
                    // Intervall aus der Liste der verbotenen Positionen entfernen
                    intervals.Value.Remove(interval);
                }
            }
        }
    }

    /// <summary> Aktualisiert die verbotenen Zeiten und Positionen vom Schiff </summary>
    /// <param name="key"> Der Schlüssel von Bibliothek </param>
    /// <param name="value"> Der Wert unter dem Schlüssel in Bibliothek </param>
    /// <param name="intervalType"> Der Typ des Intervalls (position / time) </param>
    private void UpdateForbiddenIntervals(int key, Tuple<int, int> value, string intervalType)
    {
        Dictionary<int, List<Tuple<int, int>>>? forbiddenIntervals = intervalType.Equals("time")
            ? forbiddenTimes
            : intervalType.Equals("position")
                ? forbiddenPositions
                : null;
        if (forbiddenIntervals == null)
        {
            throw new Exception("Invalid interval type");
        }
        else
        {
            // Wenn es schon ein Schlüssel mit der Anlegezeit des aktuellen Schiffes gibt
            if (forbiddenIntervals.ContainsKey(key))
            {
                // Zu diesem Schlüssel das Intervall der Position des aktuellen Schiffes hinzufügen
                forbiddenIntervals[key].Add(value);
            }
            else
            {
                // Ansonsten einen neuen Schlüssel mit dem Intervall der Position des aktuellen Schiffes hinzufügen
                forbiddenIntervals.Add(key, new List<Tuple<int, int>> { value });
            }
        }
    }

    /// <summary>
    /// Überprüft die Überlappung von zwei Intervallen.
    /// </summary>
    /// <param name="sp1">Startzeitpunkt des 1. Interval</param>
    /// <param name="ep1">Endzeitpunkt des 1. Intervals</param>
    /// <param name="sp2">Startzeitpunkt des 2. Interval</param>
    /// <param name="ep2">Endzeitpunkt des 2. Intervals</param>
    /// <returns>True, wenn eine Überlappung gefunden wird</returns>
    public static bool PosOverlap(int sp1, int ep1, int sp2, int ep2)
    {
        if ((sp2 <= sp1 && sp1 < ep2) || (ep2 >= ep1 && ep1 > sp2 || (sp1 <= sp2 && ep2 <= ep1)))
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Eine tiefe Kopie von einem Array erstellen
    /// </summary>
    /// <param name="originalArray">Das zu kopierendes Array</param
    /// <returns>Kopie vom Array als eine Liste</returns>
    public static List<int> DeepCopyArrayToList(int[] originalArray)
    {
        int[] clonedArray = new int[originalArray.Length];
        Array.Copy(originalArray, clonedArray, originalArray.Length);
        List<int> newList = clonedArray.ToList();

        return newList;
    }

    /// <summary>
    /// Optimiert die Positionen der Schiffe
    /// </summary>
    /// <param name="v">Das zu optimierende Schiff</param>
    public void OptimizePositions(Ship v)
    {
        int furthestPosition = quay.Capacity - v.Size; // take furthest possible position in current line
        int soonestArrival = 0; // find soonest arrival time of ship v
        bool optimalArrivalFound = false; // Flag, ob optimale Anlegezeit gefunden wurde
        int maxShipPosition = int.MaxValue; // Maximale Position eines Schiffes
        //Dictionary<int, List<Tuple<int, int>>> optimalTimes = new(); // Liste mit den optimalen Positionen
        //foreach (var kpvT in forbiddenTimes)
        //{
        //    foreach (var intervalT in kpvT.Value)
        //    {
        //        if (intervalT.Item2 == s[v.ID])
        //        {
        //            foreach (var kpvP in forbiddenPositions)
        //            {
        //                foreach (var intervalP in kpvP.Value)
        //                {
        //                    if (
        //                        PosOverlap(
        //                            intervalP.Item1,
        //                            intervalP.Item2,
        //                            b[v.ID],
        //                            b[v.ID] + v.Size
        //                        )
        //                    )
        //                    {
        //                        Tuple<int, int> value = Tuple.Create(
        //                            intervalP.Item1,
        //                            intervalP.Item2
        //                        );
        //                        if (!optimalTimes.ContainsKey(intervalT.Item2))
        //                        {
        //                            optimalTimes.Add(
        //                                intervalT.Item2,
        //                                new List<Tuple<int, int>> { value }
        //                            );
        //                        }
        //                        else
        //                        {
        //                            optimalTimes[intervalT.Item2].Add(value);
        //                        }

        //                        //soonestArrival = intervalT.Item2;
        //                        //optimalArrivalFound = true;
        //                        //arrivalIsOptimal = true;
        //                    }
        //                }
        //                if (arrivalIsOptimal)
        //                {
        //                    break;
        //                }
        //            }
        //        }
        //        if (arrivalIsOptimal)
        //        {
        //            break;
        //        }
        //    }
        //    if (arrivalIsOptimal)
        //    {
        //        break;
        //    }
        //}

        //if (optimalTimes.Count == 1)
        //{
        //    soonestArrival = optimalTimes.Keys.First();
        //}
        //else if (optimalTimes.Count > 1)
        //{
        //    foreach (var kvp1 in optimalTimes)
        //    {
        //        foreach (var kvp2 in optimalTimes)
        //        {
        //            foreach (var kvp1Interval in kvp1.Value)
        //            {
        //                foreach (var kvp2Interval in kvp2.Value)
        //                {
        //                    if (kvp1.Key.Equals(kvp2.Key) && kvp1Interval.Equals(kvp2Interval))
        //                    {
        //                        continue;
        //                    }
        //                    else
        //                    {
        //                        if (
        //                            PosOverlap(
        //                                kvp1Interval.Item1,
        //                                kvp1Interval.Item2,
        //                                kvp2Interval.Item1,
        //                                kvp2Interval.Item2
        //                            )
        //                        )
        //                        {
        //                            soonestArrival = kvp1.Key > kvp2.Key ? kvp1.Key : kvp2.Key;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
        //soonestArrival = newTime;

        //for (int i = ships.Count * 2; i != 0; i--)
        //// durch alle schiffe iterieren um sicherzustellen, dass das aktuelle schiff nicht mit einem anderen schiff kollidiert
        //{
        //    if (ships.IndexOf(v) != i && i != ships.IndexOf(v) + ships.Count)
        //    // die iteration soll nicht mit dem aktuellen schiff stattfinden
        //    {
        //        foreach (var kvpP in forbiddenPositions)
        //        // durch alle verbotenen positionen iterieren
        //        {
        //            foreach (var intervalP in kvpP.Value)
        //            // durch alle intervale der liste iterieren
        //            {
        //                foreach (var kvpT in forbiddenTimes)
        //                // durch alle verbotenen zeiten iterieren
        //                {
        //                    foreach (var intervalT in kvpT.Value)
        //                    // durch alle intervale der liste iterieren
        //                    {
        //                        if (
        //                            kvpP.Key == intervalT.Item1
        //                            && kvpT.Key == intervalP.Item1
        //                            && !optimalArrivalFound
        //                        )
        //                        // ein Schiff identifizieren
        //                        {
        //                            if (
        //                                intervalT.Item2 == s[v.ID]
        //                                && PosOverlap(
        //                                    intervalP.Item1,
        //                                    intervalP.Item2,
        //                                    b[v.ID],
        //                                    b[v.ID] + v.Size
        //                                )
        //                            )
        //                            {
        //                                soonestArrival = intervalT.Item2;
        //                                optimalArrivalFound = true;
        //                            }
        //                            else if (
        //                                PosOverlap(
        //                                    intervalP.Item1,
        //                                    intervalP.Item2,
        //                                    b[v.ID],
        //                                    b[v.ID] + v.Size
        //                                )
        //                                && PosOverlap(
        //                                    intervalT.Item1,
        //                                    intervalT.Item2,
        //                                    soonestArrival,
        //                                    soonestArrival + v.HandlingTime
        //                                )
        //                            )
        //                            // wenn ein Schiff gefunden wird, welches mit dem schiff v kollidiert
        //                            {
        //                                soonestArrival = intervalT.Item2;
        //                                //i = ships.Count * 2;
        //                            }
        //                        }
        //                        else if (
        //                            kvpP.Key == intervalT.Item1
        //                            && kvpT.Key == intervalP.Item1
        //                            && optimalArrivalFound && !optimalPositionFound
        //                        )
        //                        // ein Schiff identifizieren, überprüfen ob eine optimale Anliegezeit gefunden wurde
        //                        {
        //                            if (
        //                                PosOverlap(
        //                                    intervalP.Item1,
        //                                    intervalP.Item2,
        //                                    furthestPosition,
        //                                    furthestPosition + v.Size
        //                                )
        //                                && PosOverlap(
        //                                    intervalT.Item1,
        //                                    intervalT.Item2,
        //                                    soonestArrival,
        //                                    soonestArrival + v.HandlingTime
        //                                )
        //                                && intervalP.Item1 - v.Size < maxShipPosition
        //                            )
        //                            // wenn ein Schiff gefunden wird, welches nach dem aktuellen Schiff positioniert ist
        //                            {
        //                                if (intervalP.Item1 - v.Size < 0)
        //                                {
        //                                    soonestArrival = intervalT.Item2;
        //                                    //furthestPosition = quay.Capacity - v.Size;
        //                                }
        //                                else
        //                                {
        //                                    furthestPosition = intervalP.Item1 - v.Size; // das Schiff möglichst nah zum nächstliegenden Schiff positionieren
        //                                }
        //                            }
        //                            else if (
        //                                PosOverlap(
        //                                    intervalT.Item1,
        //                                    intervalT.Item2,
        //                                    soonestArrival,
        //                                    soonestArrival + v.HandlingTime
        //                                )
        //                                && intervalP.Item1 > b[v.ID]
        //                                && intervalP.Item1 - v.Size < maxShipPosition
        //                            )
        //                            {
        //                                furthestPosition = intervalP.Item1 - v.Size;
        //                                maxShipPosition = intervalP.Item1 - v.Size;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        optimalArrivalFound = true; // Flag setzen, dass optimale Anlegezeit gefunden wurde
        //    }
        //}

        for (int i = 0; i < 4; i++)
        // durch alle schiffe iterieren um sicherzustellen, dass das aktuelle schiff nicht mit einem anderen schiff kollidiert
        {
            //if (ships.IndexOf(v) != i && i != ships.IndexOf(v) + ships.Count)
            //// die iteration soll nicht mit dem aktuellen schiff stattfinden
            //{
            foreach (var kvpP in forbiddenPositions)
            // durch alle verbotenen positionen iterieren
            {
                foreach (var intervalP in kvpP.Value)
                // durch alle intervale der liste iterieren
                {
                    foreach (var kvpT in forbiddenTimes)
                    // durch alle verbotenen zeiten iterieren
                    {
                        foreach (var intervalT in kvpT.Value)
                        // durch alle intervale der liste iterieren
                        {
                            if (
                                kvpP.Key == intervalT.Item1
                                && kvpT.Key == intervalP.Item1
                                && !optimalArrivalFound
                            )
                            // ein Schiff identifizieren
                            {
                                if (
                                    intervalT.Item2 == s[v.ID]
                                    && PosOverlap(
                                        intervalP.Item1,
                                        intervalP.Item2,
                                        b[v.ID],
                                        b[v.ID] + v.Size
                                    )
                                )
                                {
                                    soonestArrival = intervalT.Item2;
                                    optimalArrivalFound = true;
                                }
                                else if (
                                    PosOverlap(
                                        intervalP.Item1,
                                        intervalP.Item2,
                                        b[v.ID],
                                        b[v.ID] + v.Size
                                    )
                                    && PosOverlap(
                                        intervalT.Item1,
                                        intervalT.Item2,
                                        soonestArrival,
                                        soonestArrival + v.HandlingTime
                                    )
                                )
                                // wenn ein Schiff gefunden wird, welches mit dem schiff v kollidiert
                                {
                                    soonestArrival = intervalT.Item2;
                                    //i = ships.Count * 2;
                                }
                            }
                            else if (kvpP.Key == intervalT.Item1 && kvpT.Key == intervalP.Item1)
                            // ein Schiff identifizieren, überprüfen ob eine optimale Anliegezeit gefunden wurde
                            {
                                if (
                                    PosOverlap(
                                        intervalP.Item1,
                                        intervalP.Item2,
                                        furthestPosition,
                                        furthestPosition + v.Size
                                    )
                                    && PosOverlap(
                                        intervalT.Item1,
                                        intervalT.Item2,
                                        soonestArrival,
                                        soonestArrival + v.HandlingTime
                                    )
                                    && intervalP.Item1 - v.Size < maxShipPosition
                                )
                                // wenn ein Schiff gefunden wird, welches nach dem aktuellen Schiff positioniert ist
                                {
                                    if (intervalP.Item1 - v.Size < 0)
                                    {
                                        soonestArrival = intervalT.Item2;
                                        //furthestPosition = quay.Capacity - v.Size;
                                    }
                                    else
                                    {
                                        furthestPosition = intervalP.Item1 - v.Size; // das Schiff möglichst nah zum nächstliegenden Schiff positionieren
                                    }
                                }
                                else if (
                                    PosOverlap(
                                        intervalT.Item1,
                                        intervalT.Item2,
                                        soonestArrival,
                                        soonestArrival + v.HandlingTime
                                    )
                                    && intervalP.Item1 > b[v.ID]
                                    && intervalP.Item1 - v.Size < maxShipPosition
                                )
                                {
                                    furthestPosition = intervalP.Item1 - v.Size;
                                    maxShipPosition = intervalP.Item1 - v.Size;
                                }
                            }
                        }
                    }
                }
            }
            optimalArrivalFound = true; // Flag setzen, dass optimale Anlegezeit gefunden wurde
            //}
        }
        s[v.ID] = soonestArrival; // Anlegezeit aktualisieren
        b[v.ID] = furthestPosition; // Position aktualisieren
    }

    /// <summary>
    /// Erstellt eine Liste mit Lösungen
    /// </summary>
    /// <returns>Liste mit der aktuellen Lösung</returns>
    public List<Solution> GetSolution()
    {
        List<Solution> solutions = new List<Solution>();
        foreach (Ship ship in A)
        {
            solutions.Add(
                new Solution(
                    ship.ID,
                    s[ship.ID],
                    s[ship.ID] + ship.HandlingTime,
                    b[ship.ID],
                    b[ship.ID] + ship.Size,
                    benchmarks
                )
            );
        }
        return solutions;
    }

    public void PrintSolution()
    {
        Console.WriteLine("Solution:");
        // Das Ergebnis ausgeben
        foreach (Ship a in A)
        {
            Console.WriteLine(
                "Schiff "
                    + a.ID
                    + " | AnlegeZeit: "
                    + s[a.ID]
                    + " | Bearbeitung: "
                    + a.HandlingTime
                    + " | AblegeZeit: "
                    + (s[a.ID] + a.HandlingTime)
                    + " | Liegeposition: "
                    + b[a.ID]
                    + " | Size: "
                    + a.Size
                    + " | EndPosition: "
                    + (b[a.ID] + a.Size)
            );
        }
    }
}
