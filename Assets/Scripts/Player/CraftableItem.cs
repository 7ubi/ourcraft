using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "CraftableItem", menuName = "Ourcraft/CraftableItem")]
public class CraftableItem : ScriptableObject
{
    public string name;

    public int resultID;
    public int[] recipe; //size always 9


    public List<int[]> GetRequirements()
    {
        var requirements = new List<int[]>();

        foreach (var r in recipe)
        {
            var added = false;
            
            foreach (var requirement in requirements.Where(requirement => requirement[0] == r))
            {
                requirement[1] += 1;
                added = true;
            }
            
            if(!added)
                requirements.Add(new int[] {r, 1});
        }

        return requirements;
    }
}
