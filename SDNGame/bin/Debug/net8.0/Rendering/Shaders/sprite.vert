#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aUv;
layout (location = 2) in vec4 aColor;

uniform mat4 uMVP; 

out vec2 vUv;
out vec4 vColor;

void main()
{
    gl_Position = uMVP * vec4(aPos, 0.0, 1.0);
    vUv = aUv;
    vColor = aColor;
}