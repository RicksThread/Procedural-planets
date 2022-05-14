#ifndef MATHHELPER_INCLUDED
#define MATHHELPER_INCLUDED

#define PI 3.14159265359;
#define TAU PI * 2;
#define MAXFLOAT 3.402823466e+38;

//Given a triangle in object space it calculates the normal and tangent to object space
void GetTriangleNormalAndTSToOSMatrix(float3 posOS1, float3 posOS2, float3 posOS3, out float3 normalOS, out float3x3 tangentToOS) {
    // Calculate a basis for the tangent space
    // The tangent, or X direction, points from a to b
    float3 tangentOS = normalize(posOS1 - posOS2);
    // The normal, or Z direction, is perpendicular to the lines formed by the triangle points
    normalOS = normalize(cross(tangentOS, posOS3 - posOS1));
    // The bitangent, or Y direction, is perpendicular to the tangent and normal
    float3 bitangentOS = normalize(cross(tangentOS, normalOS));
    // Now we can construct a tangent -> object rotation matrix
    tangentToOS = transpose(float3x3(tangentOS, bitangentOS, -normalOS));
}

//Given a triangle in object space it calculates the normal and tangent to object space
void GetTriangleNormalAndTSToWSMatrix(float3 posWS1, float3 posWS2, float3 posWS3, out float3 normalWS, out float3x3 tangentToWS) {
    // Calculate a basis for the tangent space
    // The tangent, or X direction, points from a to b
    float3 tangentWS = normalize(posWS1 - posWS2);
    // The normal, or Z direction, is perpendicular to the lines formed by the triangle points
    normalWS = normalize(cross(tangentWS, posWS3 - posWS1));
    // The bitangent, or Y direction, is perpendicular to the tangent and normal
    float3 bitangentWS = normalize(cross(tangentWS, normalWS));
    // Now we can construct a tangent -> object rotation matrix
    tangentToWS = transpose(float3x3(tangentWS, bitangentWS, normalWS));
}

// Returns the center point of a triangle defined by the three arguments
float3 GetTriangleCenter(float3 a, float3 b, float3 c) {
    return (a + b + c) / 3.0;
}
float2 GetTriangleCenter(float2 a, float2 b, float2 c) {
    return (a + b + c) / 3.0;
}

// Returns a pseudorandom number. By Ronja Böhringer
float rand(float4 value) {
    float4 smallValue = sin(value);
    float random = dot(smallValue, float4(12.9898, 78.233, 37.719, 9.151));
    random = frac(sin(random) * 143758.5453);
    return random;
}

float rand(float3 pos, float offset) {
    return rand(float4(pos, offset));
}

float randNegative1to1(float3 pos, float offset) {
    return rand(pos, offset) * 2 - 1;
}

// A function to compute an rotation matrix which rotates a point
// by angle radians around the given axis
// By Keijiro Takahashi
float3x3 AngleAxis3x3(float angle, float3 axis) {
    float c, s;
    sincos(angle, s, c);

    float t = 1 - c;
    float x = axis.x;
    float y = axis.y;
    float z = axis.z;

    return float3x3(
        t * x * x + c, t * x * y - s * z, t * x * z + s * y,
        t * x * y + s * z, t * y * y + c, t * y * z - s * x,
        t * x * z - s * y, t * y * z + s * x, t * z * z + c
        );
}


//returns the distance to the atmosphere in x and the dst through the atmosphere in y
float2 raySphere(float3 centre, float radius, float3 rayOrigin, float3 rayDir)
{
    float3 offSet = rayOrigin - centre;
    const float a = 1;
    float b = 2 * dot(offSet, rayDir);
    float c = dot(offSet,offSet) - radius * radius;

    //find the delta of the second grade equation
    float discriminant = b*b-4*a*c;

    if (discriminant > 0)
    {

        float s = sqrt(discriminant);

        float dstToSphereNear = max(0,(-b-s) / (2*a));
        float dstToSphereFar = (-b+s) / (2*a);

        if (dstToSphereFar >= 0)
        {
            return float2(dstToSphereNear, dstToSphereFar - dstToSphereNear);
        }
    }
    return float2(0,0);
}



#endif