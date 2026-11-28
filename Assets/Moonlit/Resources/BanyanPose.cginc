// Inverse-map each pose to the ink of the original woodcut. The wrist follows
// the shoulder, while the hanging roots settle after the crown comes to rest.
float2 BanyanPoseUV(float2 uv, float2 pose, float2 texel)
{
    if (pose.y <= 0.001) return uv;
    float beat = frac(pose.x / 3.2);
    float inhale = smoothstep(.00,.30,beat) - smoothstep(.34,.72,beat);
    float gather = smoothstep(.04,.24,beat) - smoothstep(.27,.55,beat);
    float answer = smoothstep(.25,.51,beat) - smoothstep(.55,.84,beat);
    float settle = smoothstep(.55,.72,beat) - smoothstep(.76,1.,beat);
    float left = (1.-smoothstep(.20,.42,uv.x)) * smoothstep(.24,.48,uv.y) * (1.-smoothstep(.77,.88,uv.y));
    float right = smoothstep(.58,.80,uv.x) * smoothstep(.24,.46,uv.y) * (1.-smoothstep(.75,.88,uv.y));
    float2 l = uv-float2(.40,.48), r = uv-float2(.60,.47);
    float2 delta = left * float2(-l.y,l.x) * (-.23*gather+.12*answer+.055*settle)
                 + right * float2(-r.y,r.x) * (.20*answer-.12*gather-.06*settle);
    float crown = smoothstep(.57,.87,uv.y);
    float torso = smoothstep(.16,.48,uv.y)*(1.-smoothstep(.62,.89,uv.y));
    delta += float2(crown*(.014*gather-.022*answer+.008*settle),
                    -.022*crown*inhale + .016*torso*inhale);
    float looseRoots = smoothstep(.13,.38,abs(uv.x-.50))*smoothstep(.20,.33,uv.y)*(1.-smoothstep(.42,.57,uv.y));
    delta.x += looseRoots * (.018*sin(6.283*beat - 8.*uv.y + 3.*uv.x) + .014*(answer-settle));
    // The outermost painted leaves already touch the sprite margin. Let their
    // tips travel vertically while easing horizontal travel into that margin.
    delta.x *= smoothstep(0., .055, min(uv.x, 1.-uv.x));
    float2 source = uv-round((delta*pose.y)/texel)*texel;

    // Pair of carved eyelids briefly closes over the amber inset in the face.
    float eyes = max(1.-smoothstep(.009,.015,abs(source.x-.467)),
                     1.-smoothstep(.009,.015,abs(source.x-.538)));
    float blink = (smoothstep(.66,.686,beat)-smoothstep(.714,.751,beat))*pose.y;
    if (eyes * blink > .01 && source.y > .550 - .022*blink*eyes && source.y < .550)
        source.y += .031;
    return source;
}
