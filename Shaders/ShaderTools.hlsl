#include "./SimplexNoise3D.hlsl"

void SimplexNoise3D_float(
    float3 input,
    float scale,
    float3 offset,
    out float output01
) {
    float3 pos = input * scale + offset;
    output01 = snoise(pos) * .5 + .5;
}

float2 GetOffsetNoise2D(
    float3 worldPos,
    float frequency,
    float scale,
    float time
) {
    float nx = snoise(worldPos * scale + float3(1, 0, 0) * time * frequency);
    float ny = snoise(worldPos * scale + float3(0, 1, 0) * time * frequency);
    return float2(nx, ny) * 2 - 1;
}

void OffsetNoise2D_float(
    float3 worldPos,
    float frequency,
    float scale,
    float time,
    out float2 output
) {
    output = GetOffsetNoise2D(worldPos, frequency, scale, time);
}

float3 GetOffsetNoise3D(
    float3 worldPos,
    float frequency,
    float scale,
    float time
) {
    float nx = snoise(worldPos * scale + float3(1, 0, 0) * time * frequency);
    float ny = snoise(worldPos * scale + float3(0, 1, 0) * time * frequency);
    float nz = snoise(worldPos * scale + float3(0, 0, 1) * time * frequency);
    return float3(nx, ny, nz) * 2 - 1;
}

void OffsetNoise3D_float(
    float3 worldPos,
    float frequency,
    float scale,
    float time,
    out float3 output
) {
    output = GetOffsetNoise3D(worldPos, frequency, scale, time);
}
