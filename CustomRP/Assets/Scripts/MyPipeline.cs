using UnityEngine;
using UnityEngine.Rendering;
using Conditional = System.Diagnostics.ConditionalAttribute;

public class MyPipeline : RenderPipeline
{
    CommandBuffer buffer = new CommandBuffer() { name = "Render Camera" };

    Material errorMaterial;

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            Render(context, camera);
        }
    }

    /// <summary>
    /// �����������Ⱦ����
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    void Render(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters cullingParameters;
        //�������ȡ�޳�����
        if (!camera.TryGetCullingParameters(out cullingParameters))
            return;

        //���UI��scene����
#if UNITY_EDITOR

        if (camera.cameraType == CameraType.SceneView)
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        
#endif

        //�����޳�����
        CullingResults cullResults = context.Cull(ref cullingParameters);

        //���������Ӧ����������
        context.SetupCameraProperties(camera);

        //����һ���������

        CameraClearFlags clearFlags = camera.clearFlags;
        //�����ȾĿ�꣺�Ƿ���������Ϣ���Ƿ������ɫ���ݣ���������ɫ��ı�����ɫ
        buffer.ClearRenderTarget((clearFlags & CameraClearFlags.Depth) != 0, (clearFlags & CameraClearFlags.Color) != 0, camera.backgroundColor);

        buffer.BeginSample("Render Camera");
        //ִ�л������е�����,��������ִ�У���ʮ�����Ǹ��Ƶ������ĵ��ڲ�������
        context.ExecuteCommandBuffer(buffer);
        //�ͷŻ�������������Ҫ����ԴҪ�����ͷ�����
        buffer.Clear();

        //��Ⱦ�ɼ�������
        //ʹ��Ĭ�ϵ�Unlitͨ������ͨ���� SRPDefaultUnlit ��ʶ
        ShaderTagId shaderTagId = new ShaderTagId("SRPDefaultUnlit");
        SortingSettings sortingSettings = new SortingSettings(camera);
        //��Ⱦ˳��Ϊ��ǰ�������
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        var drawSettings = new DrawingSettings(shaderTagId, sortingSettings);
        var filterSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);

        //��Ⱦ��պ�
        context.DrawSkybox(camera);

        //͸������Ӻ���ǰ����
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawSettings.sortingSettings = sortingSettings;
        //��Ⱦ��Χ����ΪRenderQueueRange.transparent
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);


        DrawDefaultPipeline(context, camera, cullResults);

        buffer.EndSample("Render Camera");
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
        //��Ϊ�����������ǻ���ģ�������Ҫͨ��submit���佻��ִ��
        context.Submit();
    }

    /// <summary>
    /// ��ȾĬ�ϲ��ʵķ�������ʾ��֧�ֵ���ɫ��
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]//ִ������
    void DrawDefaultPipeline(ScriptableRenderContext context, Camera camera, CullingResults cullResults)
    {
        if (errorMaterial == null)
        {
            Shader errorShader = Shader.Find("Hidden/InternalErrorShader");
            errorMaterial = new Material(errorShader) { hideFlags = HideFlags.HideAndDontSave };
        }
        ShaderTagId shaderTagId = new ShaderTagId("ForwardBase");
        SortingSettings sortingSettings = new SortingSettings(camera);
        var drawSettings = new DrawingSettings(shaderTagId, sortingSettings);

        drawSettings.SetShaderPassName(1, new ShaderTagId("PrepassBase"));
        drawSettings.SetShaderPassName(2, new ShaderTagId("Always"));
        drawSettings.SetShaderPassName(3, new ShaderTagId("Vertex"));
        drawSettings.SetShaderPassName(4, new ShaderTagId("VertexLMRGBM"));
        drawSettings.SetShaderPassName(5, new ShaderTagId("VertexLM"));

        drawSettings.overrideMaterial = errorMaterial;

        var filterSettings = FilteringSettings.defaultValue;

        context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);
    }
}
