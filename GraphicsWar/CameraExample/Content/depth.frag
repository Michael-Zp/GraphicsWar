#version 430 core


in Data {
	float depth;
} i;

out float depth;

void main() 
{
	depth = i.depth;
}