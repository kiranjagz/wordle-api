-- Seed completed games for leaderboard demo data
INSERT INTO games (game_id, player_name, secret_word, status, attempts_used, score, started_at, completed_at)
VALUES
    ('a1000000-0000-0000-0000-000000000001', 'kiran',   'crane', 1, 2, 940, NOW() - INTERVAL '2 hours',   NOW() - INTERVAL '2 hours' + INTERVAL '60 seconds'),
    ('a1000000-0000-0000-0000-000000000002', 'alice',   'brave', 1, 3, 680, NOW() - INTERVAL '1 hour',    NOW() - INTERVAL '1 hour' + INTERVAL '120 seconds'),
    ('a1000000-0000-0000-0000-000000000003', 'bob',     'stoic', 1, 4, 520, NOW() - INTERVAL '45 minutes', NOW() - INTERVAL '45 minutes' + INTERVAL '80 seconds'),
    ('a1000000-0000-0000-0000-000000000004', 'kiran',   'flame', 1, 1, 1000, NOW() - INTERVAL '30 minutes', NOW() - INTERVAL '30 minutes' + INTERVAL '5 seconds'),
    ('a1000000-0000-0000-0000-000000000005', 'alice',   'grape', 1, 5, 370, NOW() - INTERVAL '20 minutes', NOW() - INTERVAL '20 minutes' + INTERVAL '300 seconds'),
    ('a1000000-0000-0000-0000-000000000006', 'charlie', 'music', 1, 3, 695, NOW() - INTERVAL '15 minutes', NOW() - INTERVAL '15 minutes' + INTERVAL '50 seconds'),
    ('a1000000-0000-0000-0000-000000000007', 'bob',     'plant', 2, 6, NULL, NOW() - INTERVAL '10 minutes', NOW() - INTERVAL '10 minutes' + INTERVAL '180 seconds'),
    ('a1000000-0000-0000-0000-000000000008', 'kiran',   'world', 1, 2, 845, NOW() - INTERVAL '5 minutes',  NOW() - INTERVAL '5 minutes' + INTERVAL '55 seconds')
ON CONFLICT (game_id) DO NOTHING;

-- Seed guesses for the completed games
INSERT INTO guesses (guess_id, game_id, attempt_number, word, result_json, guessed_at)
VALUES
    -- kiran's game 1 (crane, 2 attempts)
    ('b1000000-0000-0000-0000-000000000001', 'a1000000-0000-0000-0000-000000000001', 1, 'slate',
     '[{"Letter":"s","Position":0,"Result":"Absent"},{"Letter":"l","Position":1,"Result":"Absent"},{"Letter":"a","Position":2,"Result":"Correct"},{"Letter":"t","Position":3,"Result":"Absent"},{"Letter":"e","Position":4,"Result":"Correct"}]',
     NOW() - INTERVAL '2 hours' + INTERVAL '30 seconds'),
    ('b1000000-0000-0000-0000-000000000002', 'a1000000-0000-0000-0000-000000000001', 2, 'crane',
     '[{"Letter":"c","Position":0,"Result":"Correct"},{"Letter":"r","Position":1,"Result":"Correct"},{"Letter":"a","Position":2,"Result":"Correct"},{"Letter":"n","Position":3,"Result":"Correct"},{"Letter":"e","Position":4,"Result":"Correct"}]',
     NOW() - INTERVAL '2 hours' + INTERVAL '60 seconds'),

    -- alice's game 2 (brave, 3 attempts)
    ('b1000000-0000-0000-0000-000000000003', 'a1000000-0000-0000-0000-000000000002', 1, 'crane',
     '[{"Letter":"c","Position":0,"Result":"Absent"},{"Letter":"r","Position":1,"Result":"Present"},{"Letter":"a","Position":2,"Result":"Present"},{"Letter":"n","Position":3,"Result":"Absent"},{"Letter":"e","Position":4,"Result":"Correct"}]',
     NOW() - INTERVAL '1 hour' + INTERVAL '40 seconds'),
    ('b1000000-0000-0000-0000-000000000004', 'a1000000-0000-0000-0000-000000000002', 2, 'drape',
     '[{"Letter":"d","Position":0,"Result":"Absent"},{"Letter":"r","Position":1,"Result":"Correct"},{"Letter":"a","Position":2,"Result":"Correct"},{"Letter":"p","Position":3,"Result":"Absent"},{"Letter":"e","Position":4,"Result":"Correct"}]',
     NOW() - INTERVAL '1 hour' + INTERVAL '80 seconds'),
    ('b1000000-0000-0000-0000-000000000005', 'a1000000-0000-0000-0000-000000000002', 3, 'brave',
     '[{"Letter":"b","Position":0,"Result":"Correct"},{"Letter":"r","Position":1,"Result":"Correct"},{"Letter":"a","Position":2,"Result":"Correct"},{"Letter":"v","Position":3,"Result":"Correct"},{"Letter":"e","Position":4,"Result":"Correct"}]',
     NOW() - INTERVAL '1 hour' + INTERVAL '120 seconds'),

    -- kiran's perfect game (flame, 1 attempt)
    ('b1000000-0000-0000-0000-000000000006', 'a1000000-0000-0000-0000-000000000004', 1, 'flame',
     '[{"Letter":"f","Position":0,"Result":"Correct"},{"Letter":"l","Position":1,"Result":"Correct"},{"Letter":"a","Position":2,"Result":"Correct"},{"Letter":"m","Position":3,"Result":"Correct"},{"Letter":"e","Position":4,"Result":"Correct"}]',
     NOW() - INTERVAL '30 minutes' + INTERVAL '5 seconds')
ON CONFLICT (guess_id) DO NOTHING;

-- Refresh the materialized view so leaderboard reflects seeded data
REFRESH MATERIALIZED VIEW CONCURRENTLY leaderboard_rankings;
