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
    /// 单个相机的渲染方法
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    void Render(ScriptableRenderContext context, Camera camera)
    {
        ScriptableCullingParameters cullingParameters;
        //从相机获取剔除参数
        if (!camera.TryGetCullingParameters(out cullingParameters))
            return;

        //添加UI在scene窗口
#if UNITY_EDITOR

        if (camera.cameraType == CameraType.SceneView)
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        
#endif

        //安排剔除操作
        CullingResults cullResults = context.Cull(ref cullingParameters);

        //将相机属性应用于上下文
        context.SetupCameraProperties(camera);

        //创建一个命令缓冲区

        CameraClearFlags clearFlags = camera.clearFlags;
        //清除渲染目标：是否清除深度信息，是否清除颜色数据，如果清除颜色后的背景颜色
        buffer.ClearRenderTarget((clearFlags & CameraClearFlags.Depth) != 0, (clearFlags & CameraClearFlags.Color) != 0, camera.backgroundColor);

        buffer.BeginSample("Render Camera");
        //执行缓冲区中的命令,不会立刻执行，二十将它们复制到上下文的内部缓冲区
        context.ExecuteCommandBuffer(buffer);
        //释放缓冲区，不再需要的资源要立刻释放它们
        buffer.Clear();

        //渲染可见的内容
        //使用默认的Unlit通道，该通道由 SRPDefaultUnlit 标识
        ShaderTagId shaderTagId = new ShaderTagId("SRPDefaultUnlit");
        SortingSettings sortingSettings = new SortingSettings(camera);
        //渲染顺序为从前往后绘制
        sortingSettings.criteria = SortingCriteria.CommonOpaque;
        var drawSettings = new DrawingSettings(shaderTagId, sortingSettings);
        var filterSettings = FilteringSettings.defaultValue;
        context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);

        //渲染天空盒
        context.DrawSkybox(camera);

        //透明物体从后往前绘制
        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawSettings.sortingSettings = sortingSettings;
        //渲染范围更改为RenderQueueRange.transparent
        filterSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullResults, ref drawSettings, ref filterSettings);


        DrawDefaultPipeline(context, camera, cullResults);

        buffer.EndSample("Render Camera");
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
        //因为发出的命令是缓冲的，所以需要通过submit将其交互执行
        context.Submit();
    }

    /// <summary>
    /// 渲染默认材质的方法，显示不支持的着色器
    /// </summary>
    /// <param name="context"></param>
    /// <param name="camera"></param>
    [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]//执行条件
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
