using System;
using UnityEngine;

[Serializable]
public class API_Response_Data
{
    public int draw_id;
    public string updated_at;
    public string created_at;
    public int id;
    public string intermediateResults; // •¶š—ñŒ^‚Ö•ÏX
    public string presentation;
    public string revival;
    public string finalResults;
}

[Serializable]
public class IntermediateResults
{
    public int[] results;
}

[Serializable]
public class Presentation
{
    public int presentationId;
    public int revivalId;
}

[Serializable]
public class Revival
{
    public int nth;
    public int changeInto;
}

[Serializable]
public class FinalResults
{
    public int[] results;
}